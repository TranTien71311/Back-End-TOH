using Newtonsoft.Json;
using SpeedTOHAPI.Codes;
using SpeedTOHAPI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Xml.Linq;

namespace SpeedTOHAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class DietaryPropertiesController : ApiController
    {
        [HttpPost]
        public APIResult POSTDietaryProperties([NakedBody] string body)
        {
            APIResult result = new APIResult();
            OdbcConnection conPixelSqlbase = new OdbcConnection();
            try
            {
                if (!Request.Headers.Contains("PartnerKey"))
                {
                    result.Status = 206;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }
                string PartnerKey = Request.Headers.GetValues("PartnerKey").First();
                conPixelSqlbase.ConnectionString = "DSN=" + Globals.Base64Decode(PartnerKey);

                if (!Request.Headers.Contains("Token"))
                {
                    result.Status = 203;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }
                string Token = Request.Headers.GetValues("Token").First();

                if (!Globals.HMACSHA256(Request.RequestUri.PathAndQuery + "." + body, System.Configuration.ConfigurationManager.AppSettings["SecretKey"].ToString()).Equals(Token))
                {
                    result.Status = 202;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }

                string[] AccessAPI = System.Configuration.ConfigurationManager.AppSettings["AccessAPI"].ToString().Split(',');
                if (!AccessAPI.Contains(System.Reflection.MethodBase.GetCurrentMethod().Name) && AccessAPI[0] != "*" && !AccessAPI.Contains(this.ControllerContext.RouteData.Values["controller"].ToString() + ".*"))
                {
                    result.Status = 201;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }

                List<DietaryPropertieModel> Dietarys = JsonConvert.DeserializeObject<List<DietaryPropertieModel>>(body);

                if (Dietarys.Count() < 0)
                {
                    result.Status = 501;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }
                if( Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["MaxLengthAPI"]) < Dietarys.Count())
                {
                    result.Status = 208;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }
                int rowIndex = 0;
                List<ErrorModel> Errors = new List<ErrorModel>();

                OdbcCommand command = new OdbcCommand();
                try
                {
                    conPixelSqlbase.Open();
                    command.Connection = conPixelSqlbase;
                }
                catch
                {
                    result.Status = 207;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }

                OdbcTransaction odbcTransact = null;
                try
                {
                    odbcTransact = conPixelSqlbase.BeginTransaction();
                    command.Transaction = odbcTransact;

                    foreach (var dietary in Dietarys)
                    {
                        rowIndex++;

                        if (dietary.VisitCode == null)
                        {
                            result.Status = 502;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        if (dietary.HN == null)
                        {
                            result.Status = 503;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        command.CommandText = @"SELECT isnull(PatientID,0)
                                        FROM dba.Patients
                                        WHERE VisitCode = '" + dietary.VisitCode + @"'
                                        AND HN = '" + dietary.HN + @"'
                                        AND IsActive = 1";
                        int PatientID = (int)command.ExecuteScalar();

                        if (PatientID == 0)
                        {
                            result.Status = 504;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        command.CommandText = @"SELECT COUNT(DietaryID)
                                        FROM dba.DietaryProperties
                                        WHERE VisitCode = '" + dietary.VisitCode + @"'
                                        AND HN = '" + dietary.HN + @"'
                                        AND IsActive = 1";
                        int CountDietary = (int)command.ExecuteScalar();

                        if (CountDietary > 0)
                        {
                            result.Status = 510;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        if (dietary.ValidFrom == null)
                        {
                            result.Status = 505;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        if (dietary.ValidTo == null)
                        {
                            result.Status = 506;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        if (dietary.DietCode == null)
                        {
                            result.Status = 507;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        //if (dietary.KitchenCode != null)
                        //{
                        //    command.CommandText = @"SELECT COUNT(KitchenID)
                        //                FROM dba.Kitchens
                        //                WHERE KitchenCode = '" + dietary.KitchenCode + @"'
                        //                AND IsActive = 1";
                        //    int CountKitchen = (int)command.ExecuteScalar(); 
                            
                        //    if(CountKitchen == 0)
                        //    {
                        //        result.Status = 511;
                        //        var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                        //        Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                        //        continue;
                        //    }
                        //}
                        //if (dietary.PantryCode != null)
                        //{
                        //    command.CommandText = @"SELECT COUNT(PantryID)
                        //                FROM dba.Pantrys
                        //                WHERE PantryCode = '" + dietary.PantryCode + @"'
                        //                AND IsActive = 1";
                        //    int CountPantry = (int)command.ExecuteScalar();

                        //    if (CountPantry == 0)
                        //    {
                        //        result.Status = 512;
                        //        var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                        //        Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                        //        continue;
                        //    }
                        //}
                        //if (dietary.SnackCode != null)
                        //{
                        //    command.CommandText = @"SELECT COUNT(SnackID)
                        //                FROM dba.Snacks
                        //                WHERE SnackCode = '" + dietary.SnackCode + @"'
                        //                AND IsActive = 1";
                        //    int CountSnack = (int)command.ExecuteScalar();

                        //    if (CountSnack == 0)
                        //    {
                        //        result.Status = 513;
                        //        var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                        //        Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                        //        continue;
                        //    }
                        //}

                        command.CommandText = @"SELECT isnull(MAX(DietaryID),0)
                                        FROM dba.DietaryProperties";
                        int DietaryID = (int)command.ExecuteScalar() + 1;


                        command.CommandText = @"INSERT INTO dba.DietaryProperties(DietaryID,VisitCode, HN, FoodTexture, Comments, KitchenCode, PantryCode, SnackCode, ValidFrom, ValidTo, PatientID)
                                                VALUES(?,?,?,?,?,?,?,?,?,?,?)";
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("DietaryID", DietaryID);
                        command.Parameters.AddWithValue("VisitCode", dietary.VisitCode.ToString());
                        command.Parameters.AddWithValue("HN", dietary.HN.ToString());
                        command.Parameters.AddWithValue("FoodTexture", dietary.FoodTexture != null ? dietary.FoodTexture.ToString() : null);
                        command.Parameters.AddWithValue("Comments", dietary.Comments != null ? dietary.Comments.ToString() : null);
                        command.Parameters.AddWithValue("KitchenCode", dietary.KitchenCode != null ? dietary.KitchenCode.ToString() : null);
                        command.Parameters.AddWithValue("PantryCode", dietary.PantryCode != null ? dietary.PantryCode.ToString() : null);
                        command.Parameters.AddWithValue("SnackCode", dietary.SnackCode != null ? dietary.SnackCode.ToString() : null);
                        command.Parameters.AddWithValue("ValidFrom", Convert.ToDateTime(dietary.ValidFrom).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture));
                        command.Parameters.AddWithValue("ValidTo", Convert.ToDateTime(dietary.ValidTo).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture));
                        command.Parameters.AddWithValue("PatientID", PatientID);
                        command.ExecuteNonQuery();

                        //Insert MenuType
                        foreach(var dietcode in dietary.DietCode)
                        {
                            command.CommandText = @"SELECT COUNT(DietID)
                                        FROM dba.Diets
                                        WHERE DietCode = '"+ dietcode + @"'
                                        AND IsActive = 1";
                            int CountDietCode = (int)command.ExecuteScalar();
                            if (CountDietCode == 0)
                            {
                                result.Status = 508;
                                var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                continue;
                            }

                            command.CommandText = @"INSERT INTO dba.MenuTypes(PatientID,DietCode)
                                                VALUES(?,?)";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("PatientID", PatientID);
                            command.Parameters.AddWithValue("DietCode", dietcode.ToString());
                            command.ExecuteNonQuery();
                        }

                        //if(dietary.FoodAllergies != null)
                        //{
                        //    //Insert FoodAllergies
                        //    foreach (var foodcode in dietary.FoodAllergies)
                        //    {
                        //        command.CommandText = @"SELECT COUNT(FoodID)
                        //                FROM dba.Foods
                        //                WHERE FoodCode = '" + foodcode + @"'
                        //                AND IsActive = 1";
                        //        int CountFoodCode = (int)command.ExecuteScalar();
                        //        if (CountFoodCode == 0)
                        //        {
                        //            result.Status = 509;
                        //            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                        //            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                        //            continue;
                        //        }

                        //        command.CommandText = @"INSERT INTO dba.FoodAllergies(PatientID,FoodCode)
                        //                        VALUES(?,?)";
                        //        command.Parameters.Clear();
                        //        command.Parameters.AddWithValue("PatientID", PatientID);
                        //        command.Parameters.AddWithValue("FoodCode", foodcode.ToString());
                        //        command.ExecuteNonQuery();
                        //    }
                        //}
                        
                    }
                    if (Errors.Count > 0)
                    {
                        odbcTransact.Rollback();
                        result.Error = Errors;
                    }
                    else
                    {
                        result.Status = 200;
                        var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                        result.Message = msg != null ? msg.Message : "";
                        odbcTransact.Commit();
                    }
                }
                catch (Exception ex)
                {
                    if (odbcTransact != null)
                        odbcTransact.Rollback();
                    result.Status = 0;
                    result.Message = ex.Message;
                    result.Exception = ex.ToString();
                }

            }
            catch (Exception ex)
            {
                result.Status = 205;
                result.Message = ex.Message;
                result.Exception = ex.ToString();
            }
            finally
            {
                conPixelSqlbase.Close();
            }
            return result;
        }
        [HttpPut]
        public APIResult PUTDietaryProperties([NakedBody] string body)
        {
            APIResult result = new APIResult();
            OdbcConnection conPixelSqlbase = new OdbcConnection();
            try
            {
                if (!Request.Headers.Contains("PartnerKey"))
                {
                    result.Status = 206;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }
                string PartnerKey = Request.Headers.GetValues("PartnerKey").First();
                conPixelSqlbase.ConnectionString = "DSN=" + Globals.Base64Decode(PartnerKey);

                if (!Request.Headers.Contains("Token"))
                {
                    result.Status = 203;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }
                string Token = Request.Headers.GetValues("Token").First();
                if (!Globals.HMACSHA256(Request.RequestUri.PathAndQuery + "." + body, System.Configuration.ConfigurationManager.AppSettings["SecretKey"].ToString()).Equals(Token))
                {
                    result.Status = 202;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }

                string[] AccessAPI = System.Configuration.ConfigurationManager.AppSettings["AccessAPI"].ToString().Split(',');
                if (!AccessAPI.Contains(System.Reflection.MethodBase.GetCurrentMethod().Name) && AccessAPI[0] != "*" && !AccessAPI.Contains(this.ControllerContext.RouteData.Values["controller"].ToString() + ".*"))
                {
                    result.Status = 201;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }

                List<DietaryPropertieModel> Dietarys = JsonConvert.DeserializeObject<List<DietaryPropertieModel>>(body);

                if (Dietarys.Count() < 0)
                {
                    result.Status = 501;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }
                if (Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["MaxLengthAPI"]) < Dietarys.Count())
                {
                    result.Status = 208;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }
                int rowIndex = 0;
                List<ErrorModel> Errors = new List<ErrorModel>();

                OdbcCommand command = new OdbcCommand();
                try
                {
                    conPixelSqlbase.Open();
                    command.Connection = conPixelSqlbase;
                }
                catch
                {
                    result.Status = 207;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }

                OdbcTransaction odbcTransact = null;
                try
                {
                    odbcTransact = conPixelSqlbase.BeginTransaction();
                    command.Transaction = odbcTransact;

                    foreach (var dietary in Dietarys)
                    {
                        rowIndex++;

                        if (dietary.VisitCode == null)
                        {
                            result.Status = 502;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        if (dietary.HN == null)
                        {
                            result.Status = 503;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        command.CommandText = @"SELECT isnull(DietaryID,0)
                                        FROM dba.DietaryProperties
                                        WHERE VisitCode = '" + dietary.VisitCode + @"'
                                        AND HN = '" + dietary.HN + @"'
                                        AND IsActive = 1";
                        int DietaryID = (int)command.ExecuteScalar();

                        if (DietaryID == 0)
                        {
                            result.Status = 504;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        string query = "UPDATE DBA.DietaryProperties SET ModifiedDate= ? ";

                        if (dietary.ValidFrom != null)
                        {
                            query += ", ValidFrom='" + Convert.ToDateTime(dietary.ValidFrom).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) + "'";
                        }
                        if (dietary.ValidTo != null)
                        {
                            query += ", ValidTo='" + Convert.ToDateTime(dietary.ValidTo).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) + "'";
                        }
                        if (dietary.FoodTexture != null)
                        {
                            query += ", FoodTexture='" + dietary.FoodTexture + "'";
                        }
                        if (dietary.Comments != null)
                        {
                            query += ", Comments='" + dietary.Comments + "'";
                        }
                        if (dietary.KitchenCode != null)
                        {
                            command.CommandText = @"SELECT COUNT(KitchenID)
                                        FROM dba.Kitchens
                                        WHERE KitchenCode = '" + dietary.KitchenCode + @"'
                                        AND IsActive = 1";
                            int CountKitchen = (int)command.ExecuteScalar();

                            if (CountKitchen == 0)
                            {
                                result.Status = 511;
                                var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                continue;
                            }
                            query += ", KitchenCode='" + dietary.KitchenCode + "'";
                        }
                        if (dietary.PantryCode != null)
                        {
                            command.CommandText = @"SELECT COUNT(PantryID)
                                    FROM dba.Pantrys
                                    WHERE PantryCode = '" + dietary.PantryCode + @"'
                                    AND IsActive = 1";
                            int CountPantry = (int)command.ExecuteScalar();

                            if (CountPantry == 0)
                            {
                                result.Status = 512;
                                var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                continue;
                            }
                            query += ", PantryCode='" + dietary.PantryCode + "'";
                        }
                        if (dietary.SnackCode != null)
                        {
                            command.CommandText = @"SELECT COUNT(SnackID)
                                    FROM dba.Snacks
                                    WHERE SnackCode = '" + dietary.SnackCode + @"'
                                    AND IsActive = 1";
                            int CountSnack = (int)command.ExecuteScalar();

                            if (CountSnack == 0)
                            {
                                result.Status = 513;
                                var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                continue;
                            }
                            query += ", SnackCode='" + dietary.SnackCode + "'";
                        }
                        if (dietary.IsActive != null)
                        {
                            query += ", IsActive=" + Convert.ToInt32(dietary.IsActive) + "";
                        }
                        query += " WHERE VisitCode='" + dietary.VisitCode + "' AND HN='" + dietary.HN + "'";

                        command.CommandText = query;
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("ModifiedDate", Convert.ToDateTime(DateTime.Now).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture));
                        command.ExecuteNonQuery();

                        
                        if (dietary.DietCode != null)
                        {

                            command.CommandText = @"SELECT PatientID
                                        FROM dba.Patients
                                        WHERE VisitCode = '" + dietary.VisitCode + @"'
                                          AND HN = '" + dietary.HN + @"'
                                          AND IsActive = 1";
                            int PatientID = (int)command.ExecuteScalar();

                            command.CommandText = @"UPDATE dba.MenuTypes SET IsActive=0
                                                    WHERE PatientID = ?";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("PatientID", PatientID);
                            command.ExecuteNonQuery();

                            foreach (var dietcode in dietary.DietCode)
                            {
                                command.CommandText = @"SELECT COUNT(DietID)
                                        FROM dba.Diets
                                        WHERE DietCode = '" + dietcode + @"'
                                        AND IsActive = 1";
                                int CountDietCode = (int)command.ExecuteScalar();
                                if (CountDietCode == 0)
                                {
                                    result.Status = 508;
                                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                    Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                    continue;
                                }

                                command.CommandText = @"INSERT INTO dba.MenuTypes(PatientID,DietCode)
                                                VALUES(?,?)";
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("PatientID", PatientID);
                                command.Parameters.AddWithValue("DietCode", dietcode.ToString());
                                command.ExecuteNonQuery();
                            }
                        }
                        

                        if (dietary.FoodAllergies != null)
                        {
                            command.CommandText = @"SELECT PatientID
                                        FROM dba.Patients
                                        WHERE VisitCode = '" + dietary.VisitCode + @"'
                                          AND HN = '" + dietary.HN + @"'
                                          AND IsActive = 1";
                            int PatientID = (int)command.ExecuteScalar();

                            command.CommandText = @"UPDATE dba.FoodAllergies SET IsActive=0
                                                    WHERE PatientID = ?";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("PatientID", PatientID);
                            command.ExecuteNonQuery();

                            //Insert FoodAllergies
                            foreach (var foodcode in dietary.FoodAllergies)
                            {
                                command.CommandText = @"SELECT COUNT(FoodID)
                                        FROM dba.Foods
                                        WHERE FoodCode = '" + foodcode + @"'
                                        AND IsActive = 1";
                                int CountFoodCode = (int)command.ExecuteScalar();
                                if (CountFoodCode == 0)
                                {
                                    result.Status = 509;
                                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                    Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                    continue;
                                }

                                command.CommandText = @"INSERT INTO dba.FoodAllergies(PatientID,FoodCode)
                                                VALUES(?,?)";
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("PatientID", PatientID);
                                command.Parameters.AddWithValue("FoodCode", foodcode.ToString());
                                command.ExecuteNonQuery();
                            }
                        }

                    }
                    if (Errors.Count > 0)
                    {
                        odbcTransact.Rollback();
                        result.Error = Errors;
                    }
                    else
                    {
                        result.Status = 200;
                        var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                        result.Message = msg != null ? msg.Message : "";
                        odbcTransact.Commit();
                    }
                }
                catch (Exception ex)
                {
                    if (odbcTransact != null)
                        odbcTransact.Rollback();
                    result.Status = 0;
                    result.Message = ex.Message;
                    result.Exception = ex.ToString();
                }

            }
            catch (Exception ex)
            {
                result.Status = 205;
                result.Message = ex.Message;
                result.Exception = ex.ToString();
            }
            finally
            {
                conPixelSqlbase.Close();
            }
            return result;
        }

        [HttpGet]
        public APIResult GETDietaryProperties(Nullable<int> PantientID = null,
                                       string VisitCode = null,
                                       string HN = null,
                                       string KitchenCode = null,
                                       string PantryCode = null,
                                       string SnackCode = null,
                                       string ValidFrom = null,
                                       string ValidTo = null,
                                       Nullable<int> PageSize = null,
                                       Nullable<int> PageNum = null,
                                       Nullable<bool> IsActive = null,
                                       string OrderBy = null)
        {
            APIResult result = new APIResult();
            OdbcConnection conPixelSqlbase = new OdbcConnection();
            try
            {
                if (!Request.Headers.Contains("PartnerKey"))
                {
                    result.Status = 206;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }
                string PartnerKey = Request.Headers.GetValues("PartnerKey").First();
                conPixelSqlbase.ConnectionString = "DSN=" + Globals.Base64Decode(PartnerKey);

                if (!Request.Headers.Contains("Token"))
                {
                    result.Status = 203;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }
                string Token = Request.Headers.GetValues("Token").First();
                string Token1 = Request.RequestUri.PathAndQuery;
                if (!Globals.HMACSHA256(Request.RequestUri.PathAndQuery, System.Configuration.ConfigurationManager.AppSettings["SecretKey"].ToString()).Equals(Token))
                {
                    result.Status = 202;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }

                string[] AccessAPI = System.Configuration.ConfigurationManager.AppSettings["AccessAPI"].ToString().Split(',');
                if (!AccessAPI.Contains(System.Reflection.MethodBase.GetCurrentMethod().Name) && AccessAPI[0] != "*" && !AccessAPI.Contains(this.ControllerContext.RouteData.Values["controller"].ToString() + ".*"))
                {
                    result.Status = 201;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }
                OdbcCommand command = new OdbcCommand();
                try
                {
                    conPixelSqlbase.Open();
                    command.Connection = conPixelSqlbase;
                }
                catch
                {
                    result.Status = 207;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }
                int _PageSize = 1000;
                int _PageNum = 0;
                if (PageNum != null)
                {
                    _PageNum = Convert.ToInt32(PageNum);
                }
                if (PageSize != null)
                {
                    _PageSize = Convert.ToInt32(PageSize);
                }
                string queryin = @"SELECT TOP " + _PageSize + @" START AT " + (_PageNum == 0 ? 1 : ((_PageNum * _PageSize) + 1)) + @" 
                                    d.PatientID AS 'PatientID'
                                    FROM DBA.DietaryProperties d
                                    WHERE PatientID <> 0";

                string query = @"SELECT TOP " + _PageSize + @" START AT " + (_PageNum == 0 ? 1 : ((_PageNum * _PageSize) + 1)) + @" 
                                    d.PatientID AS 'PatientID', 
                                    d.VisitCode AS 'VisitCode', 
                                    d.HN AS 'HN',
                                    d.FoodTexture AS 'FoodTexture', 
                                    d.Comments AS 'Comments', 
                                    d.KitchenCode AS 'KitchenCode', 
                                    f.KitchenName AS 'KitchenName', 
                                    d.PantryCode AS 'PantryCode',
                                    p.PantryName AS 'PantryName',
                                    d.SnackCode AS 'SnackCode',
                                    s.SnackName AS 'SnackName',
                                    d.ValidFrom AS 'ValidFrom',
                                    d.ValidFrom AS 'ValidTo',
                                    d.IsActive AS 'IsActive',
                                    d.CreatedDate AS 'CreatedDate',
                                    d.ModifiedDate AS 'ModifiedDate'
                                    FROM DBA.DietaryProperties d
                                    LEFT JOIN DBA.Kitchens f ON d.KitchenCode = f.KitchenCode
                                    LEFT JOIN DBA.Pantrys p ON d.PantryCode = p.PantryCode
                                    LEFT JOIN DBA.Snacks s ON d.SnackCode = s.SnackCode
                                    WHERE PatientID <> 0";

                if (PantientID != null)
                {
                    query += " AND d.PantientID = " + Convert.ToInt32(PantientID) + "";
                    queryin += " AND d.PantientID = " + Convert.ToInt32(PantientID) + "";
                }
                if (VisitCode != null)
                {
                    query += " AND d.VisitCode = '" + VisitCode + "'";
                    queryin += " AND d.VisitCode = '" + VisitCode + "'";
                }
                if (HN != null)
                {
                    query += " AND d.HN = '" + HN + "'";
                    queryin += " AND d.HN = '" + HN + "'";
                }
                if (KitchenCode != null)
                {
                    query += " AND d.KitchenCode = '" + KitchenCode + "'";
                    queryin += " AND d.KitchenCode = '" + KitchenCode + "'";
                }
                if (PantryCode != null)
                {
                    query += " AND d.PantryCode = '" + PantryCode + "'";
                    queryin += " AND d.PantryCode = '" + PantryCode + "'";
                }
                if (SnackCode != null)
                {
                    query += " AND d.SnackCode = '" + SnackCode + "'";
                    queryin += " AND d.SnackCode = '" + SnackCode + "'";
                }
                if (ValidFrom != null)
                {
                    DateTime Date = DateTime.Parse(ValidFrom);
                    query += " AND d.ValidFrom >= '" + Date.ToString("yyyy/MM/dd 00:00:00", CultureInfo.InvariantCulture) + "'";
                    queryin += " AND d.ValidFrom >= '" + Date.ToString("yyyy/MM/dd 00:00:00", CultureInfo.InvariantCulture) + "'";
                }
                if (ValidTo != null)
                {
                    DateTime Date = DateTime.Parse(ValidTo);
                    query += " AND d.ValidTo <= '" + Date.ToString("yyyy/MM/dd 23:59:59", CultureInfo.InvariantCulture) + "'";
                    queryin += " AND d.ValidTo <= '" + Date.ToString("yyyy/MM/dd 23:59:59", CultureInfo.InvariantCulture) + "'";
                }
                
                if (IsActive != null)
                {
                    query += " AND d.IsActive = " + (IsActive == true ? 1 : 0) + "";
                    queryin += " AND d.IsActive = " + (IsActive == true ? 1 : 0) + "";
                }
                string _OrderBy = "ASC";
                if (OrderBy == "DESC")
                {
                    _OrderBy = "DESC";
                }
                query += " ORDER BY d.PatientID " + _OrderBy + "";
                command.CommandText = query;
                DataTable Data = new DataTable("Patients");
                Data.Load(command.ExecuteReader());
                List<DietaryPropertieModel> DietaryProperties = JsonConvert.DeserializeObject<List<DietaryPropertieModel>>(JsonConvert.SerializeObject(Data));

                string queryMenuType = "SELECT * FROM DBA.MenuTypes WHERE PatientID IN (" + queryin + ")";
                command.CommandText = queryMenuType;
                DataTable DataMenuTypes = new DataTable("MenuTypes");
                DataMenuTypes.Load(command.ExecuteReader());
                List<MenuTypeModel> MenuTypes = JsonConvert.DeserializeObject<List<MenuTypeModel>>(JsonConvert.SerializeObject(DataMenuTypes));

                string queryFoodAll = "SELECT * FROM DBA.FoodAllergies WHERE PatientID IN (" + queryin + ")";
                command.CommandText = queryFoodAll;
                DataTable DataFoodAllergies = new DataTable("FoodAllergies");
                DataFoodAllergies.Load(command.ExecuteReader());
                List<FoodAllergieModel> FoodAllergies = JsonConvert.DeserializeObject<List<FoodAllergieModel>>(JsonConvert.SerializeObject(DataFoodAllergies));

                var JoinData = (from data in DietaryProperties
                                select new
                                {
                                    PatientID =  data.PatientID,
                                    VisitCode = data.VisitCode,
                                    HN = data.HN,
                                    FoodTexture = data.FoodTexture,
                                    Comments = data.Comments,
                                    KitchenCode = data.KitchenCode,
                                    KitchenName = data.KitchenName,
                                    PantryCode = data.PantryCode,
                                    PantryName = data.PantryName,
                                    SnackCode = data.SnackCode,
                                    SnackName = data.SnackName,
                                    ValidFrom = data.ValidFrom,
                                    ValidTo = data.ValidTo,
                                    IsActive = data.IsActive,
                                    CreatedDate = data.CreatedDate,
                                    ModifiedDate = data.ModifiedDate,
                                    FoodAllergiesList = FoodAllergies.Where(x=>x.PatientID == data.PatientID).ToList(),
                                    MenuTypes = MenuTypes.Where(x=>x.PatientID == data.PatientID).ToList()
                                }).ToList();

                result.Status = 200;
                result.Message = "OK";
                result.Data = JoinData;
            }
            catch (Exception ex)
            {
                result.Status = 205;
                result.Message = ex.Message;
                result.Exception = ex.ToString();
            }
            finally
            {
                conPixelSqlbase.Close();
            }
            return result;
        }
    }
}