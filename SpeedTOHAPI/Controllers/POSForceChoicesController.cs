using Newtonsoft.Json;
using SpeedTOHAPI.Codes;
using SpeedTOHAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace SpeedTOHAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class POSForceChoicesController : ApiController
    {
        [HttpPut]
        public APIResult Put([NakedBody] string body)
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

                List<POSForcedChoiceModel> ForcedChoices = JsonConvert.DeserializeObject<List<POSForcedChoiceModel>>(body);
                if (ForcedChoices.Count() < 0)
                {
                    result.Status = 605;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }
                if (Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["MaxLengthAPI"]) < ForcedChoices.Count())
                {
                    result.Status = 208;
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

                    foreach (var forcedchoice in ForcedChoices)
                    {
                        rowIndex++;
                        if (forcedchoice.UniqueID == null)
                        {
                            result.Status = 606;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        else
                        {
                            command.CommandText = @"SELECT ISNULL(UniqueID, -1)
                                        FROM dba.POSForcedChoices
                                        WHERE UniqueID = '" + forcedchoice.UniqueID + @"'";
                            int UniqueID = (int)command.ExecuteScalar();
                            if (UniqueID == -1)
                            {
                                result.Status = 619;
                                var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                continue;
                            }
                            //UPDATE
                            try
                            {
                                string query = "UPDATE DBA.POSForcedChoices SET ModifiedDate= ? ";
                                query += " WHERE UniqueID='" + forcedchoice.UniqueID + "'";

                                command.CommandText = query;
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("ModifiedDate", Convert.ToDateTime((DateTime.Now).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture)));
                                command.ExecuteNonQuery();

                                if (forcedchoice.Translations.Count() > 0)
                                {
                                    foreach (var translation in forcedchoice.Translations)
                                    {
                                        if (translation.TranslationType == null)
                                        {
                                            result.Status = 617;
                                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                            continue;
                                        }
                                        if (translation.TranslationText == null || translation.TranslationText == "")
                                        {
                                            result.Status = 618;
                                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                            continue;
                                        }
                                        command.CommandText = @"SELECT ISNULL(TranslationID, -1)
                                        FROM dba.Translations
                                        WHERE TranslationID = '" + translation.TranslationID + @"'";
                                        int TranslationID = (int)command.ExecuteScalar();
                                        if (TranslationID == -1)
                                        {
                                            result.Status = 619;
                                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                            continue;
                                        }
                                        command.CommandText = @"SELECT ISNULL(TranslationPOSForceChoiceID, -1)
                                        FROM dba.TranslationPOSForceChoices
                                        WHERE TranslationID = '" + translation.TranslationID + @"' AND UniqueID = '" + forcedchoice.UniqueID + "'";
                                        int TranslationPOSForceChoiceID = (int)command.ExecuteScalar();
                                        if (TranslationPOSForceChoiceID == -1)
                                        {
                                            //Insert
                                            command.CommandText = @"INSERT INTO DBA.TranslationPOSForceChoices (TranslationID, UniqueID, TranslationType, TranslationText)
                                                                    VALUES (?,?,?,?)";
                                            command.Parameters.Clear();
                                            command.Parameters.AddWithValue("TranslationID", Convert.ToInt32(translation.TranslationID));
                                            command.Parameters.AddWithValue("UniqueID", Convert.ToInt32(translation.UniqueID));
                                            command.Parameters.AddWithValue("TranslationType", Convert.ToInt32(translation.TranslationType));
                                            command.Parameters.AddWithValue("TranslationText", translation.TranslationText.ToString());
                                            command.ExecuteNonQuery();
                                        }
                                        else
                                        {
                                            //Update
                                            command.CommandText = @"UPDATE DBA.TranslationPOSForceChoices SET TranslationID = ?, UniqueID = ?, TranslationType = ?, TranslationText = ?
                                                                    WHERE TranslationPOSForceChoiceID = ?";
                                            command.Parameters.Clear();
                                            command.Parameters.AddWithValue("TranslationID", Convert.ToInt32(translation.TranslationID));
                                            command.Parameters.AddWithValue("UniqueID", Convert.ToInt32(translation.UniqueID));
                                            command.Parameters.AddWithValue("TranslationType", Convert.ToInt32(translation.TranslationType));
                                            command.Parameters.AddWithValue("TranslationText", translation.TranslationText.ToString());
                                            command.Parameters.AddWithValue("TranslationPOSForceChoiceID", Convert.ToInt32(TranslationPOSForceChoiceID));
                                            command.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                var msg = new MessageModel { Status = 205, Function = "POSProduct", Message = ex.Message, Description = ex.ToString() };
                                Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
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
                        result.Data = ForcedChoices;
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
        public APIResult Get(Nullable<int> ForcedChoiceID = null
                            , Nullable<int> UniqueID = null
                            , Nullable<int> OptionIndex = null
                            , Nullable<int> Sequence = null
                            , Nullable<int> ChoiceProductNum = null
                            , Nullable<bool> IsActive = null
                            , Nullable<int> PageSize = null
                            , Nullable<int> PageNum = null
                            , string OrderBy = null)
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
                                    p.UniqueID AS 'UniqueID'
                                    FROM DBA.POSForcedChoices f
                                    WHERE ForcedChoiceID <> 0";

                string query = @"SELECT TOP " + _PageSize + @" START AT " + (_PageNum == 0 ? 1 : ((_PageNum * _PageSize) + 1)) + @" 
                                    p.ForcedChoiceID AS 'ForcedChoiceID',
                                    p.UniqueID AS 'UniqueID',
                                    p.OptionIndex AS 'OptionIndex',
                                    p.Question AS 'Question',
                                    p.Sequence AS 'Sequence',
                                    p.Descript AS 'Descript',
                                    p.ChoiceProductNum AS 'ChoiceProductNum',
                                    p.ChoiceProductName AS 'ChoiceProductName',
                                    p.Price AS 'Price',
                                    p.DateCreated AS 'DateCreated',
                                    p.DateModified AS 'DateModified',
                                    p.IsActive AS 'IsActive',
                                    FROM DBA.POSProductCombos p
                                    WHERE ForcedChoiceID <> 0";

                if (ForcedChoiceID != null)
                {
                    query += " AND p.ForcedChoiceID = " + Convert.ToInt32(ForcedChoiceID) + "";
                    queryin += " AND p.ForcedChoiceID = " + Convert.ToInt32(ForcedChoiceID) + "";
                }
                if (UniqueID != null)
                {
                    query += " AND p.UniqueID = '" + UniqueID + "'";
                    queryin += " AND p.UniqueID = '" + UniqueID + "'";
                }
                if (OptionIndex != null)
                {
                    query += " AND p.OptionIndex = '" + OptionIndex + "'";
                    queryin += " AND p.OptionIndex = '" + OptionIndex + "'";
                }
                if (Sequence != null)
                {
                    query += " AND p.Sequence = '" + Sequence + "'";
                    queryin += " AND p.Sequence = '" + Sequence + "'";
                }
                if (Sequence != null)
                {
                    query += " AND p.ChoiceProductNum = '" + ChoiceProductNum + "'";
                    queryin += " AND p.ChoiceProductNum = '" + ChoiceProductNum + "'";
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
                query += " ORDER BY p.UniqueID " + _OrderBy + "";
                command.CommandText = query;
                DataTable Data = new DataTable("ForcedChoices");
                Data.Load(command.ExecuteReader());
                List<POSForcedChoiceModel> ForcedChoices = JsonConvert.DeserializeObject<List<POSForcedChoiceModel>>(JsonConvert.SerializeObject(Data));

                string queryTranlation = "SELECT * FROM DBA.TranlationPOSForcedChoices WHERE UniqueID IN (" + queryin + ")";
                command.CommandText = queryTranlation;
                DataTable DataTranlations = new DataTable("Tranlations");
                DataTranlations.Load(command.ExecuteReader());
                List<TranslationPOSForceChoiceModel> Tranlations = JsonConvert.DeserializeObject<List<TranslationPOSForceChoiceModel>>(JsonConvert.SerializeObject(DataTranlations));

                var JoinData = (from data in ForcedChoices
                                select new
                                {
                                    ForcedChoiceID = data.ForcedChoiceID,
                                    UniqueID = data.UniqueID,
                                    OptionIndex = data.OptionIndex,
                                    Question = data.Question,
                                    Sequence = data.Sequence,
                                    Descript = data.Descript,
                                    ChoiceProductNum = data.ChoiceProductNum,
                                    ChoiceProductName = data.ChoiceProductName,
                                    Price = data.Price,
                                    DateCreated = data.DateCreated,
                                    DateModified = data.DateModified,
                                    IsActive = data.IsActive,
                                    Tranlations = Tranlations.Where(x => x.UniqueID == data.UniqueID).ToList(),
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
