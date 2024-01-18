using Newtonsoft.Json;
using SpeedTOHAPI.Codes;
using SpeedTOHAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Data;
using System.Drawing.Printing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Web.Http;
using System.Web.Http.Cors;

namespace SpeedTOHAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class SyncDataController : ApiController
    {
        [HttpGet]
        public APIResult Get(Nullable<int> SyncID = null, Nullable<bool> IsActive = null)
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
                conPixelSqlbase.Open();
                command.Connection = conPixelSqlbase;
               
                
                string query = @"SELECT * FROM DBA.SyncData s
                                    WHERE s.SyncID <> 0";

                if (SyncID != null)
                {
                    query += " AND s.SyncID = " + Convert.ToInt32(SyncID) + "";
                }
                if (IsActive != null)
                {
                    query += " AND s.IsActive = " + (IsActive == true ? 1 : 0 )+ "";
                }
                query += " ORDER BY s.SyncID DESC";
                command.CommandText = query;
                DataTable Data = new DataTable("SyncData");
                Data.Load(command.ExecuteReader());
                List<SyncDataModel> SyncData = JsonConvert.DeserializeObject<List<SyncDataModel>>(JsonConvert.SerializeObject(Data));
                int Count = SyncData.Count != 0 ? SyncData.Count : 1;
                int TotalPages = 1;
                if(Count > 100)
                {
                    if(Count % 100 == 0)
                    {
                        TotalPages = Count / 100;
                    }
                    else
                    {
                        TotalPages = (int)(Count/100) + 1;
                    }
                }
                result.Status = 200;
                result.Message = "OK";
                result.TotalPages = TotalPages;
                result.Data = SyncData.ToList();
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
