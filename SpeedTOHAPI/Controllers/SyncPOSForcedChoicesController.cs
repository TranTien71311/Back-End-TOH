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
    public class SyncPOSForcedChoicesController : ApiController
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

                    foreach (var ForcedChoice in ForcedChoices)
                    {
                        rowIndex++;
                        if (ForcedChoice.UniqueID == null)
                        {
                            result.Status = 606;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        else
                        {
                            command.CommandText = @"SELECT Count(UniqueID)
                                        FROM dba.POSForcedchoices
                                        WHERE UniqueID = '" + ForcedChoice.UniqueID + @"'";
                            int CountUniqueID = (int)command.ExecuteScalar();
                            if (CountUniqueID > 0)
                            {
                                //UPDATE
                                try
                                {
                                    command.CommandText = @"UPDATE dba.POSForcedchoices
                                                SET OptionIndex = ?, Question = ?,
                                                Descript = ?,Sequence = ?,ChoiceProductNum = ?,ChoiceProductName = ?,
                                                Price = ?, IsActive = ?, DateModified = ?
                                                WHERE UniqueID = ?";
                                    command.Parameters.Clear();
                                    command.Parameters.AddWithValue("OptionIndex", Convert.ToInt32(ForcedChoice.OptionIndex));
                                    command.Parameters.AddWithValue("Question", ForcedChoice.Question.ToString());
                                    command.Parameters.AddWithValue("Descript", ForcedChoice.Descript.ToString());
                                    command.Parameters.AddWithValue("Sequence", Convert.ToInt32(ForcedChoice.Sequence));
                                    command.Parameters.AddWithValue("ChoiceProductNum", Convert.ToInt32(ForcedChoice.ChoiceProductNum));
                                    command.Parameters.AddWithValue("ChoiceProductName", ForcedChoice.ChoiceProductName.ToString());
                                    command.Parameters.AddWithValue("Price", Convert.ToDouble(ForcedChoice.Price));
                                    command.Parameters.AddWithValue("IsActive", Convert.ToBoolean(ForcedChoice.IsActive));
                                    command.Parameters.AddWithValue("DateModified", Convert.ToDateTime((DateTime.Now).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture)));
                                    command.Parameters.AddWithValue("UniqueID", Convert.ToInt32(ForcedChoice.UniqueID));
                                    command.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    var msg = new MessageModel { Status = 205, Function = "SyncPOSForcedChoices", Message = ex.Message, Description = ex.ToString() };
                                    Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                }
                                continue;
                            }
                        }
                        try
                        {
                            command.CommandText = @"INSERT INTO dba.POSForcedchoices
                                                (UniqueID,OptionIndex, Question,
                                                Descript,Sequence,ChoiceProductNum,ChoiceProductName,
                                                Price, IsActive)
                                                VALUES(?,?,?,?,?,?,?,?,?)";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("UniqueID", Convert.ToInt32(ForcedChoice.UniqueID));
                            command.Parameters.AddWithValue("OptionIndex", Convert.ToInt32(ForcedChoice.OptionIndex));
                            command.Parameters.AddWithValue("Question", ForcedChoice.Question.ToString());
                            command.Parameters.AddWithValue("Descript", ForcedChoice.Descript.ToString());
                            command.Parameters.AddWithValue("Sequence", Convert.ToInt32(ForcedChoice.Sequence));
                            command.Parameters.AddWithValue("ChoiceProductNum", Convert.ToInt32(ForcedChoice.ChoiceProductNum));
                            command.Parameters.AddWithValue("ChoiceProductName", ForcedChoice.ChoiceProductName.ToString());
                            command.Parameters.AddWithValue("Price", Convert.ToDouble(ForcedChoice.Price));
                            command.Parameters.AddWithValue("IsActive", Convert.ToBoolean(ForcedChoice.IsActive));
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            var msg = new MessageModel { Status = 205, Function = "SyncPOSForcedChoices", Message = ex.Message, Description = ex.ToString() };
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
    }
}
