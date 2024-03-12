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
    public class SyncPOSReportCatsController : ApiController
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
                        if (reportcat.ReportNo == null)
                        {
                            result.Status = 610;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        else
                        {
                            command.CommandText = @"SELECT Count(ReportNo)
                                        FROM dba.POSReportCats
                                        WHERE ReportNo = '" + reportcat.ReportNo + @"'";
                            int CountReportNo = (int)command.ExecuteScalar();
                            if (CountReportNo > 0)
                            {
                                //UPDATE
                                try
                                {
                                    command.CommandText = @"UPDATE dba.POSReportCats
                                                SET ReportName = ?,SummaryNum = ?, SummaryName = ?, 
                                                Course = ?, DateModified = ?,IsActive = ?
                                                WHERE ReportNo = ?";
                                    command.Parameters.Clear();
                                    command.Parameters.AddWithValue("ReportName", reportcat.ReportName.ToString());
                                    command.Parameters.AddWithValue("SummaryNum", Convert.ToInt32(reportcat.SummaryNum));
                                    command.Parameters.AddWithValue("SummaryName", reportcat.SummaryName.ToString());
                                    command.Parameters.AddWithValue("Course", (reportcat.Course != null ? Convert.ToBoolean(reportcat.Course) : false));
                                    command.Parameters.AddWithValue("DateModified", Convert.ToDateTime((DateTime.Now).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture)));
                                    command.Parameters.AddWithValue("IsActive", Convert.ToBoolean(reportcat.IsActive));
                                    command.Parameters.AddWithValue("ReportNo", Convert.ToInt32(reportcat.ReportNo));
                                    command.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    var msg = new MessageModel { Status = 205, Function = "SyncPOSReportCats", Message = ex.Message, Description = ex.ToString() };
                                    Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                }
                                continue;
                            }
                        }
                        try
                        {
                            command.CommandText = @"INSERT INTO dba.POSReportCats
                                                (ReportNo, ReportName,SummaryNum, SummaryName,Course,IsActive)
                                                VALUES(?,?,?,?,?,?)";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("ReportNo", Convert.ToInt32(reportcat.ReportNo));
                            command.Parameters.AddWithValue("ReportName", reportcat.ReportName.ToString());
                            command.Parameters.AddWithValue("SummaryNum", Convert.ToInt32(reportcat.SummaryNum));
                            command.Parameters.AddWithValue("SummaryName", reportcat.SummaryName.ToString());
                            command.Parameters.AddWithValue("Course", (reportcat.Course != null ? Convert.ToBoolean(reportcat.Course) : false));
                            command.Parameters.AddWithValue("IsActive", Convert.ToBoolean(reportcat.IsActive));
                            
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            var msg = new MessageModel { Status = 205, Function = "SyncPOSReportCats", Message = ex.Message, Description = ex.ToString() };
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
    }
}
