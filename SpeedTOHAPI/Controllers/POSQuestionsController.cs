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
    public class POSQuestionsController : ApiController
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

                List<POSQuestionModel> Questions = JsonConvert.DeserializeObject<List<POSQuestionModel>>(body);
                if (Questions.Count() < 0)
                {
                    result.Status = 607;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }
                if (Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["MaxLengthAPI"]) < Questions.Count())
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

                    foreach (var question in Questions)
                    {
                        rowIndex++;
                        if (question.OptionIndex == null)
                        {
                            result.Status = 608;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        else
                        {
                            command.CommandText = @"SELECT ISNULL(OptionIndex, -1)
                                        FROM dba.POSQuestions
                                        WHERE OptionIndex = '" + question.OptionIndex + @"'";
                            int OptionIndex = (int)command.ExecuteScalar();
                            if (OptionIndex == -1)
                            {
                                result.Status = 623;
                                var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                continue;
                            }
                            //UPDATE
                            try
                            {
                                string query = "UPDATE DBA.POSQuestions SET ModifiedDate= ? ";
                                query += " WHERE OptionIndex='" + question.OptionIndex + "'";

                                command.CommandText = query;
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("ModifiedDate", Convert.ToDateTime((DateTime.Now).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture)));
                                command.ExecuteNonQuery();

                                if (question.Translations.Count() > 0)
                                {
                                    foreach (var translation in question.Translations)
                                    {
                                        if (translation.TranslationType == null)
                                        {
                                            result.Status = 624;
                                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                            continue;
                                        }
                                        if (translation.TranslationText == null || translation.TranslationText == "")
                                        {
                                            result.Status = 625;
                                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                            continue;
                                        }
                                        command.CommandText = @"SELECT ISNULL(OptionIndex, -1)
                                                                FROM dba.Translations
                                                                WHERE OptionIndex = '" + translation.OptionIndex + @"'";
                                        int TranslationID = (int)command.ExecuteScalar();
                                        if (TranslationID == -1)
                                        {
                                            result.Status = 626;
                                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                            continue;
                                        }
                                        command.CommandText = @"SELECT ISNULL(TranslationPOSQuestionID, -1)
                                                                FROM dba.TranslationPOSQuestions
                                                                WHERE TranslationID = '" + translation.TranslationID + @"' AND OptionIndex = '" + question.OptionIndex + "'";
                                        int TranslationPOSQuestionID = (int)command.ExecuteScalar();
                                        if (TranslationPOSQuestionID == -1)
                                        {
                                            //Insert
                                            command.CommandText = @"INSERT INTO DBA.TranslationPOSQuestions (TranslationID, OptionIndex, TranslationType, TranslationText)
                                                                    VALUES (?,?,?,?)";
                                            command.Parameters.Clear();
                                            command.Parameters.AddWithValue("TranslationID", Convert.ToInt32(translation.TranslationID));
                                            command.Parameters.AddWithValue("OptionIndex", Convert.ToInt32(translation.OptionIndex));
                                            command.Parameters.AddWithValue("TranslationType", Convert.ToInt32(translation.TranslationType));
                                            command.Parameters.AddWithValue("TranslationText", translation.TranslationText.ToString());
                                            command.ExecuteNonQuery();
                                        }
                                        else
                                        {
                                            //Update
                                            command.CommandText = @"UPDATE DBA.TranslationPOSQuestions SET TranslationID = ?, OptionIndex = ?, TranslationType = ?, TranslationText = ?
                                                                    WHERE TranslationPOSQuestionID = ?";
                                            command.Parameters.Clear();
                                            command.Parameters.AddWithValue("TranslationID", Convert.ToInt32(translation.TranslationID));
                                            command.Parameters.AddWithValue("OptionIndex", Convert.ToInt32(translation.OptionIndex));
                                            command.Parameters.AddWithValue("TranslationType", Convert.ToInt32(translation.TranslationType));
                                            command.Parameters.AddWithValue("TranslationText", translation.TranslationText.ToString());
                                            command.Parameters.AddWithValue("TranslationPOSQuestionID", Convert.ToInt32(TranslationPOSQuestionID));
                                            command.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                var msg = new MessageModel { Status = 205, Function = "POSQuestions", Message = ex.Message, Description = ex.ToString() };
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
                        result.Data = Questions;
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
        public APIResult Get(Nullable<int> QuestionID = null
                            , Nullable<int> OptionIndex = null
                            , Nullable<int> Forced = null
                            , Nullable<int> NumChoice = null
                            , Nullable<int> Allowmulti = null
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
                                    FROM DBA.POSQuestions f
                                    WHERE OptionIndex <> 0";

                string query = @"SELECT TOP " + _PageSize + @" START AT " + (_PageNum == 0 ? 1 : ((_PageNum * _PageSize) + 1)) + @" 
                                    p.QuestionID AS 'QuestionID',
                                    p.OptionIndex AS 'OptionIndex',
                                    p.Question AS 'Question',
                                    p.Forced AS 'Forced',
                                    p.NumChoice AS 'NumChoice',
                                    p.Allowmulti AS 'Allowmulti',
                                    p.DateCreated AS 'DateCreated',
                                    p.DateModified AS 'DateModified',
                                    p.IsActive AS 'IsActive',
                                    FROM DBA.POSQuestions p
                                    WHERE OptionIndex <> 0";

                if (QuestionID != null)
                {
                    query += " AND p.QuestionID = " + Convert.ToInt32(QuestionID) + "";
                    queryin += " AND p.QuestionID = " + Convert.ToInt32(QuestionID) + "";
                }
                if (OptionIndex != null)
                {
                    query += " AND p.OptionIndex = '" + OptionIndex + "'";
                    queryin += " AND p.OptionIndex = '" + OptionIndex + "'";
                }
                if (OptionIndex != null)
                {
                    query += " AND p.Forced = '" + Forced + "'";
                    queryin += " AND p.Forced = '" + Forced + "'";
                }
                if (NumChoice != null)
                {
                    query += " AND p.NumChoice = '" + NumChoice + "'";
                    queryin += " AND p.NumChoice = '" + NumChoice + "'";
                }
                if (Allowmulti != null)
                {
                    query += " AND p.Allowmulti = '" + Allowmulti + "'";
                    queryin += " AND p.Allowmulti = '" + Allowmulti + "'";
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
                query += " ORDER BY p.OptionIndex " + _OrderBy + "";
                command.CommandText = query;
                DataTable Data = new DataTable("Questions");
                Data.Load(command.ExecuteReader());
                List<POSQuestionModel> Questions = JsonConvert.DeserializeObject<List<POSQuestionModel>>(JsonConvert.SerializeObject(Data));

                string queryTranlation = "SELECT * FROM DBA.TranlationPOSQuestions WHERE OptionIndex IN (" + queryin + ")";
                command.CommandText = queryTranlation;
                DataTable DataTranlations = new DataTable("Tranlations");
                DataTranlations.Load(command.ExecuteReader());
                List<TranslationQuestionModel> Tranlations = JsonConvert.DeserializeObject<List<TranslationQuestionModel>>(JsonConvert.SerializeObject(DataTranlations));

                var JoinData = (from data in Questions
                                select new
                                {
                                    QuestionID = data.QuestionID,
                                    OptionIndex = data.OptionIndex,
                                    Question = data.Question,
                                    Descript = data.Descript,
                                    Forced = data.Forced,
                                    NumChoice = data.NumChoice,
                                    Allowmulti = data.Allowmulti,
                                    DateCreated = data.DateCreated,
                                    DateModified = data.DateModified,
                                    IsActive = data.IsActive,
                                    Tranlations = Tranlations.Where(x => x.OptionIndex == data.OptionIndex).ToList(),
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
