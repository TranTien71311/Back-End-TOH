using Newtonsoft.Json;
using SpeedTOHAPI.Codes;
using SpeedTOHAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace SpeedTOHAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class SyncPOSSaleTypesController : ApiController
    {
        [HttpPost]
        public APIResult Post([NakedBody] string body)
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

                List<POSSaleTypeModel> SaleTypes = JsonConvert.DeserializeObject<List<POSSaleTypeModel>>(body);
                if (SaleTypes.Count() < 0)
                {
                    result.Status = 633;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }
                int rowIndex = 0;
                List<ErrorModel> Errors = new List<ErrorModel>();

                OdbcCommand command = new OdbcCommand();
                conPixelSqlbase.Open();
                command.Connection = conPixelSqlbase;

                OdbcTransaction odbcTransact = null;
                try
                {
                    odbcTransact = conPixelSqlbase.BeginTransaction();
                    command.Transaction = odbcTransact;

                    foreach (var saletype in SaleTypes)
                    {
                        rowIndex++;
                        if (saletype.SaleTypeIndex == null)
                        {
                            result.Status = 634;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        else
                        {
                            command.CommandText = @"SELECT COUNT(SaleTypeIndex)
                                        FROM dba.POSSaleTypes
                                        WHERE SaleTypeIndex = '" + saletype.SaleTypeIndex + @"'";
                            int CountSaleType = (int)command.ExecuteScalar();
                            if (CountSaleType > 0)
                            {
                                //UPDATE
                                try
                                {
                                    command.CommandText = @"UPDATE dba.POSSaleTypes
                                                SET Descript = ?, ForcePrice = ?,
                                                Tax1Exempt = ?,Tax2Exempt = ?,Tax3Exempt = ?,Tax4Exempt = ?,
                                                Tax5Exempt = ?, IsActive = ?, ModifiedDate = ?
                                                WHERE SaleTypeIndex = ?";
                                    command.Parameters.Clear();
                                    command.Parameters.AddWithValue("Descript", saletype.Descript.ToString());
                                    command.Parameters.AddWithValue("ForcePrice", Convert.ToDouble(saletype.ForcePrice));
                                    command.Parameters.AddWithValue("Tax1Exempt", Convert.ToInt32(saletype.Tax1Exempt));
                                    command.Parameters.AddWithValue("Tax2Exempt", Convert.ToInt32(saletype.Tax2Exempt));
                                    command.Parameters.AddWithValue("Tax3Exempt", Convert.ToInt32(saletype.Tax3Exempt));
                                    command.Parameters.AddWithValue("Tax4Exempt", Convert.ToInt32(saletype.Tax4Exempt));
                                    command.Parameters.AddWithValue("Tax5Exempt", Convert.ToInt32(saletype.Tax5Exempt));
                                    command.Parameters.AddWithValue("IsActive", Convert.ToBoolean(saletype.IsActive));
                                    command.Parameters.AddWithValue("DateModified", Convert.ToDateTime((DateTime.Now).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture)));
                                    command.Parameters.AddWithValue("SaleTypeIndex", Convert.ToInt32(saletype.SaleTypeIndex));
                                    command.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    var msg = new MessageModel { Status = 205, Function = "SyncPOSSaleType", Message = ex.Message, Description = ex.ToString() };
                                    Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                }
                                continue;
                            }
                        }
                        try
                        {
                            command.CommandText = @"INSERT INTO dba.POSSaleTypes
                                                (SaleTypeIndex,Descript, ForcePrice,
                                                Tax1Exempt,Tax2Exempt,Tax3Exempt,Tax4Exempt,Tax5Exempt, IsActive)
                                                VALUES(?,?,?,?,?,?,?,?,?)";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("SaleTypeIndex", Convert.ToInt32(saletype.SaleTypeIndex));
                            command.Parameters.AddWithValue("Descript", saletype.Descript.ToString());
                            command.Parameters.AddWithValue("ForcePrice", Convert.ToDouble(saletype.ForcePrice));
                            command.Parameters.AddWithValue("Tax1Exempt", Convert.ToInt32(saletype.Tax1Exempt));
                            command.Parameters.AddWithValue("Tax2Exempt", Convert.ToInt32(saletype.Tax2Exempt));
                            command.Parameters.AddWithValue("Tax3Exempt", Convert.ToInt32(saletype.Tax3Exempt));
                            command.Parameters.AddWithValue("Tax4Exempt", Convert.ToInt32(saletype.Tax4Exempt));
                            command.Parameters.AddWithValue("Tax5Exempt", Convert.ToInt32(saletype.Tax5Exempt));
                            command.Parameters.AddWithValue("IsActive", Convert.ToBoolean(saletype.IsActive));
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            var msg = new MessageModel { Status = 205, Function = "SyncPOSProductCombo", Message = ex.Message, Description = ex.ToString() };
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
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
                        result.Data = SaleTypes;
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
    }
}
