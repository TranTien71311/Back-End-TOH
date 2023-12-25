using SpeedTOHAPI.Codes;
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SpeedTOHAPI.Controllers
{
    public class TestController : ApiController
    {
        [HttpGet]
        public APIResult GetEmployeeBySwipe(string Swipe)
        {
            APIResult rs = new APIResult();
            OdbcConnection conPixelSqlbase = new OdbcConnection();
            try
            {
                if (!Request.Headers.Contains("PartnerKey"))
                {
                    rs.Status = 206;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == rs.Status).SingleOrDefault();
                    rs.Message = msg != null ? msg.Message : "";
                    return rs;
                }

                string PartnerKey = Request.Headers.GetValues("PartnerKey").First();
                conPixelSqlbase.ConnectionString = "DSN=" + Globals.Base64Decode(PartnerKey);

                if (!Request.Headers.Contains("Token"))
                {
                    rs.Status = 203;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == rs.Status).SingleOrDefault();
                    rs.Message = msg != null ? msg.Message : "";
                    return rs;
                }

                string Token = Request.Headers.GetValues("Token").First();
                if (!Globals.HMACSHA256(Request.RequestUri.PathAndQuery, System.Configuration.ConfigurationManager.AppSettings["SecretKey"].ToString()).Equals(Token))
                {
                    rs.Status = 202;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == rs.Status).SingleOrDefault();
                    rs.Message = msg != null ? msg.Message : "";
                    return rs;
                }

                string[] AccessAPI = System.Configuration.ConfigurationManager.AppSettings["AccessAPI"].ToString().Split(',');
                if (!AccessAPI.Contains(System.Reflection.MethodBase.GetCurrentMethod().Name) && AccessAPI[0] != "*" && !AccessAPI.Contains(this.ControllerContext.RouteData.Values["controller"].ToString() + ".*"))
                {
                    rs.Status = 201;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == rs.Status).SingleOrDefault();
                    rs.Message = msg != null ? msg.Message : "";
                    return rs;
                }

                Swipe = Swipe.Replace("'", "''");

                conPixelSqlbase.Open();
                OdbcCommand command = new OdbcCommand();
                command.Connection = conPixelSqlbase;
                command.CommandText = @"SELECT E.EmpNum,
                                               E.EmpName,
                                               E.EmpLastName,
                                               E.StartWork,
                                               E.EndWork,
                                               E.PosName,
                                               JP.JobPos AS JobPosNum,
                                               JP.Descript AS JobPosName,
                                               E.IsActive
                                        FROM DBA.EMPLOYEE E
                                        INNER JOIN DBA.PayRoll PR ON (E.EMPNUM = PR.EMPNUM)
                                        INNER JOIN DBA.Jobpos JP ON (PR.JOBPOS = JP.JOBPOS)
                                        WHERE PR.IsPrimary = 1
                                          AND E.SWIPE = '" + Swipe + @"'";
                DataTable dtResult = new DataTable("Employee");
                dtResult.Load(command.ExecuteReader());
                rs.Status = 200;
                rs.Message = "OK";
                rs.Data = dtResult;
            }
            catch (Exception ex)
            {
                rs.Status = 205;
                rs.Message = ex.Message;
                rs.Exception = ex.ToString();

            }
            finally
            {
                conPixelSqlbase.Close();
            }
            return rs;
        }
    }
}
