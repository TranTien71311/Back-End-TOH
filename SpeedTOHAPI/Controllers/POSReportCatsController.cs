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
    public class POSReportCatsController : ApiController
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

                List<POSReportCatModel> ReportCats = JsonConvert.DeserializeObject<List<POSReportCatModel>>(body);
                if (ReportCats.Count() < 0)
                {
                    result.Status = 609;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }
                if (Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["MaxLengthAPI"]) < ReportCats.Count())
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

                    foreach (var reportcat in ReportCats)
                    {
                        rowIndex++;
                        if (reportcat.ReportCatID == null)
                        {
                            result.Status = 610;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        else
                        {
                            command.CommandText = @"SELECT ISNULL(ReportCatID, -1)
                                        FROM dba.POSReportCats
                                        WHERE ReportNo = '" + reportcat.ReportNo + @"'";
                            int ReportNo = (int)command.ExecuteScalar();
                            if (ReportNo == -1)
                            {
                                result.Status = 623;
                                var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                continue;
                            }
                            //UPDATE
                            try
                            {
                                string query = "UPDATE DBA.POSReportCats SET ModifiedDate= ? ";
                                if(reportcat.Image != null)
                                {
                                    query += ", Image = "+ reportcat.Image +"";
                                }
                                if (reportcat.Index != null)
                                {
                                    query += ", Index = " + reportcat.Index + "";
                                }
                                if (reportcat.IsPublic != null)
                                {
                                    query += ", IsPublic = " + reportcat.IsPublic + "";
                                }
                                query += " WHERE ReportNo='" + reportcat.ReportNo + "'";

                                command.CommandText = query;
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("ModifiedDate", Convert.ToDateTime((DateTime.Now).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture)));
                                command.ExecuteNonQuery();

                                if (reportcat.Translations.Count() > 0)
                                {
                                    foreach (var translation in reportcat.Translations)
                                    {
                                        if (translation.TranslationType == null)
                                        {
                                            result.Status = 627;
                                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                            continue;
                                        }
                                        if (translation.TranslationText == null || translation.TranslationText == "")
                                        {
                                            result.Status = 628;
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
                                            result.Status = 629;
                                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                            continue;
                                        }
                                        command.CommandText = @"SELECT ISNULL(TranslationPOSReportCatID, -1)
                                                                FROM dba.TranslationPOSReportCats
                                                                WHERE TranslationID = '" + translation.TranslationID + @"' AND ReportNo = '" + reportcat.ReportNo + "'";
                                        int TranslationPOSReportCatID = (int)command.ExecuteScalar();
                                        if (TranslationPOSReportCatID == -1)
                                        {
                                            //Insert
                                            command.CommandText = @"INSERT INTO DBA.TranslationPOSReportCats (TranslationID, ReportNo, TranslationType, TranslationText)
                                                                    VALUES (?,?,?,?)";
                                            command.Parameters.Clear();
                                            command.Parameters.AddWithValue("TranslationID", Convert.ToInt32(translation.TranslationID));
                                            command.Parameters.AddWithValue("ReportNo", Convert.ToInt32(translation.ReportNo));
                                            command.Parameters.AddWithValue("TranslationType", Convert.ToInt32(translation.TranslationType));
                                            command.Parameters.AddWithValue("TranslationText", translation.TranslationText.ToString());
                                            command.ExecuteNonQuery();
                                        }
                                        else
                                        {
                                            //Update
                                            command.CommandText = @"UPDATE DBA.TranslationPOSQuestions SET TranslationID = ?, ReportNo = ?, TranslationType = ?, TranslationText = ?
                                                                    WHERE TranslationPOSReportCatID = ?";
                                            command.Parameters.Clear();
                                            command.Parameters.AddWithValue("TranslationID", Convert.ToInt32(translation.TranslationID));
                                            command.Parameters.AddWithValue("ReportNo", Convert.ToInt32(translation.ReportNo));
                                            command.Parameters.AddWithValue("TranslationType", Convert.ToInt32(translation.TranslationType));
                                            command.Parameters.AddWithValue("TranslationText", translation.TranslationText.ToString());
                                            command.Parameters.AddWithValue("TranslationPOSReportCatID", Convert.ToInt32(TranslationPOSReportCatID));
                                            command.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                var msg = new MessageModel { Status = 205, Function = "POSReportCats", Message = ex.Message, Description = ex.ToString() };
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
                        result.Data = ReportCats;
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
        public APIResult Get(Nullable<int> ReportCatID = null
                            , Nullable<int> ReportNo = null
                            , Nullable<int> SummaryNum = null
                            , Nullable<bool> Course = null
                            , Nullable<int> Index = null
                            , Nullable<bool> IsPublic = null
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
                                    p.ReportNo AS 'ReportNo'
                                    FROM DBA.POSReportCats p
                                    WHERE ReportNo <> 0";

                string query = @"SELECT TOP " + _PageSize + @" START AT " + (_PageNum == 0 ? 1 : ((_PageNum * _PageSize) + 1)) + @" 
                                    p.ReportCatID AS 'ReportCatID',
                                    p.ReportNo AS 'ReportNo',
                                    p.ReportName AS 'ReportName',
                                    p.Image AS 'Image',
                                    p.SummaryNum AS 'SummaryNum',
                                    p.SummaryName AS 'SummaryName',
                                    p.Course AS 'Course',
                                    p.Index AS 'Index',
                                    p.IsPublic AS 'IsPublic',
                                    p.DateCreated AS 'DateCreated',
                                    p.DateModified AS 'DateModified',
                                    p.IsActive AS 'IsActive',
                                    FROM DBA.POSReportCats p
                                    WHERE ReportNo <> 0";

                if (ReportCatID != null)
                {
                    query += " AND p.ReportCatID = " + Convert.ToInt32(ReportCatID) + "";
                    queryin += " AND p.ReportCatID = " + Convert.ToInt32(ReportCatID) + "";
                }
                if (ReportNo != null)
                {
                    query += " AND p.ReportNo = '" + ReportNo + "'";
                    queryin += " AND p.ReportNo = '" + ReportNo + "'";
                }
                if (SummaryNum != null)
                {
                    query += " AND p.SummaryNum = '" + SummaryNum + "'";
                    queryin += " AND p.SummaryNum = '" + SummaryNum + "'";
                }
                if (Course != null)
                {
                    query += " AND p.Course = " + (Course == true ? 1 : 0) + "";
                    queryin += " AND p.Course = " + (Course == true ? 1 : 0) + "";
                }
                if (Index != null)
                {
                    query += " AND p.Index = '" + Index + "'";
                    queryin += " AND p.Index = '" + Index + "'";
                }
                if (IsActive != null)
                {
                    query += " AND p.IsActive = " + (IsActive == true ? 1 : 0) + "";
                    queryin += " AND p.IsActive = " + (IsActive == true ? 1 : 0) + "";
                }
                if (IsPublic != null)
                {
                    query += " AND p.IsPublic = " + (IsPublic == true ? 1 : 0) + "";
                    queryin += " AND p.IsPublic = " + (IsPublic == true ? 1 : 0) + "";
                }
                string _OrderBy = "ASC";
                if (OrderBy == "DESC")
                {
                    _OrderBy = "DESC";
                }
                query += " ORDER BY p.ReportNo " + _OrderBy + "";
                command.CommandText = query;
                DataTable Data = new DataTable("ReportCats");
                Data.Load(command.ExecuteReader());
                List<POSReportCatModel> ReportCats = JsonConvert.DeserializeObject<List<POSReportCatModel>>(JsonConvert.SerializeObject(Data));

                string queryTranlation = "SELECT * FROM DBA.TranlationPOSReportCats WHERE ReportNo IN (" + queryin + ")";
                command.CommandText = queryTranlation;
                DataTable DataTranlations = new DataTable("Tranlations");
                DataTranlations.Load(command.ExecuteReader());
                List<TranslationPOSReportCatModel> Tranlations = JsonConvert.DeserializeObject<List<TranslationPOSReportCatModel>>(JsonConvert.SerializeObject(DataTranlations));

                var JoinData = (from data in ReportCats
                                select new
                                {
                                    ReportCatID  = data.ReportCatID,
                                    ReportNo  = data.ReportNo,
                                    ReportName  = data.ReportName,
                                    Image  = data.Image,
                                    SummaryNum  = data.SummaryNum,
                                    SummaryName  = data.SummaryName,
                                    Course  = data.Course,
                                    Index  = data.Index,
                                    IsPublic  = data.IsPublic,
                                    DateCreated  = data.DateCreated,
                                    DateModified  = data.DateModified,
                                    IsActive  = data.IsActive,
                                    Tranlations = Tranlations.Where(x => x.ReportNo == data.ReportNo).ToList(),
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
