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
using System.Globalization;

namespace SpeedTOHAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class SyncDataController : ApiController
    {
        [HttpGet]
        public APIResult Get(Nullable<int> SyncID = null, Nullable<bool> IsActive = null, Nullable<int> PageSize = null, Nullable<int> PageNum = null)
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

                string query = @"SELECT TOP " + _PageSize + @" START AT " + (_PageNum == 0 ? 1 : ((_PageNum * _PageSize) + 1)) + @" *
                                    FROM DBA.SyncData s
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

                command.CommandText = @"SELECT COUNT(SyncID)
                                        FROM dba.SyncData
                                        WHERE IsActive = 1";
                int TotalRow = (int)command.ExecuteScalar();

                int Count = TotalRow != 0 ? TotalRow : 1;
                int TotalPages = 1;
                if (Count > _PageSize)
                {
                    if (Count % _PageSize == 0)
                    {
                        TotalPages = Count / _PageSize;
                    }
                    else
                    {
                        TotalPages = (int)(Count / _PageSize) + 1;
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
                string a = body;
                string Token = Request.Headers.GetValues("Token").First();
                string Token1 = Globals.HMACSHA256(Request.RequestUri.PathAndQuery + "." + body, System.Configuration.ConfigurationManager.AppSettings["SecretKey"].ToString());
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

                SyncDataModel Sync = JsonConvert.DeserializeObject<SyncDataModel>(body);

                OdbcCommand command = new OdbcCommand();
                conPixelSqlbase.Open();
                command.Connection = conPixelSqlbase;

                OdbcTransaction odbcTransact = null;
                try
                {
                    odbcTransact = conPixelSqlbase.BeginTransaction();
                    command.Transaction = odbcTransact;

                    if (Sync.SyncID == null)
                    {
                        result.Status = 631;
                        result.Message = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault().ToString();
                        return result;
                    }

                    command.CommandText = @"SELECT COUNT(SyncID)
                                    FROM dba.SyncData
                                    WHERE SyncID = '" + Sync.SyncID + @"'
                                        AND IsActive = 1";
                    int CountSync = (int)command.ExecuteScalar();

                    if (CountSync == 0)
                    {
                        result.Status = 632;
                        result.Message = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault().ToString();
                        return result;
                    }
                    string query = "UPDATE DBA.SyncData SET ModifiedDate= ? ";
                    if (Sync.SyncName != null)
                    {
                        query += ", SyncName='" + Sync.SyncName + "'";
                    }
                    if (Sync.APIGetLink != null)
                    {
                        query += ", APIGetLink='" + Sync.APIGetLink + "'";
                    }
                    if (Sync.APIPostLink != null)
                    {
                        query += ", APIPostLink='" + Sync.APIPostLink + "'";
                    }
                    if (Sync.SyncValue != null)
                    {
                        query += ", SyncValue='" + Sync.SyncValue + "'";
                    }
                    if (Sync.IsActive != null)
                    {
                        query += ", IsActive='" + Sync.IsActive + "'";
                    }
                    query += " WHERE SyncID='" + Sync.SyncID + "'";

                    command.CommandText = query;
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("ModifiedDate", Convert.ToDateTime((DateTime.Now).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture)));
                    command.ExecuteNonQuery();
                   
                    result.Status = 200;
                    result.Data = Sync;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    odbcTransact.Commit();
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
