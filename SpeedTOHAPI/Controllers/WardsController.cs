using Newtonsoft.Json;
using SpeedTOHAPI.Codes;
using SpeedTOHAPI.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
    public class WardsController : ApiController
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

                List<WardModel> Wards = JsonConvert.DeserializeObject<List<WardModel>>(body);
                if (Wards.Count() < 0)
                {
                    result.Status = 701;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }
                if (Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["MaxLengthAPI"]) < Wards.Count())
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

                    //string queryData = @"SELECT *
                    //                    FROM dba.Wards
                    //                    WHERE IsActive = 1";

                    foreach (var ward in Wards)
                    {
                        rowIndex++;
                        if (ward.WardID == null)
                        {
                            result.Status = 702;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }

                        command.CommandText = @"SELECT COUNT(WardID)
                                        FROM dba.Wards
                                        WHERE WardID = '" + ward.WardID + @"'
                                          AND IsActive = 1";
                        int CountWard = (int)command.ExecuteScalar();

                        if (CountWard > 0)
                        {
                            result.Status = 706;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }

                        if (ward.WardNameEn == null)
                        {
                            result.Status = 703;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        if (ward.WardNameVn == null)
                        {
                            result.Status = 704;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        command.CommandText = @"INSERT INTO dba.Wards
                                            (WardID, WardNameEn, WardNameVn)
                                            VALUES(?,?,?)";
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("WardID", ward.WardID.ToString());
                        command.Parameters.AddWithValue("WardNameEn", ward.WardNameEn.ToString());
                        command.Parameters.AddWithValue("WardNameVn", ward.WardNameVn.ToString());
                        command.ExecuteNonQuery();

                        //if(rowIndex == 1)
                        //{
                        //    queryData += " AND WardID='"+ ward.WardID.ToString() + "'";
                        //}
                        //else
                        //{
                        //    queryData += " OR WardID='"+ ward.WardID.ToString() + "'";
                        //}
                    }
                    if (Errors.Count > 0)
                    {
                        odbcTransact.Rollback();
                        result.Error = Errors;
                    }
                    else
                    {
                        //command.CommandText = queryData;
                        //DataTable Data = new DataTable("Data");
                        //Data.Load(command.ExecuteReader());

                        result.Status = 200;
                        result.Data = Wards;
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

                List<WardModel> Wards = JsonConvert.DeserializeObject<List<WardModel>>(body);
                if (Wards.Count() < 0)
                {
                    result.Status = 701;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }
                if (Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["MaxLengthAPI"]) < Wards.Count())
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

                    foreach (var ward in Wards)
                    {
                        rowIndex++;
                        if (ward.WardID == null)
                        {
                            result.Status = 702;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }

                        command.CommandText = @"SELECT COUNT(WardID)
                                        FROM dba.Wards
                                        WHERE WardID = '" + ward.WardID + @"'
                                          AND IsActive = 1";
                        int CountWard = (int)command.ExecuteScalar();

                        if (CountWard == 0)
                        {
                            result.Status = 705;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        string query = "UPDATE DBA.Wards SET ModifiedDate= ? ";
                        if (ward.WardNameVn != null)
                        {
                            query += ", WardNameVn='" + ward.WardNameVn + "'";
                        }
                        if (ward.WardNameEn != null)
                        {
                            query += ", WardNameEn='" + ward.WardNameEn + "'";
                        }
                        if (ward.IsActive != null)
                        {
                            query += ", IsActive='" + Convert.ToInt32(ward.IsActive) + "'";
                        }
                        query += " WHERE WardID='" + ward.WardID + "'";

                        command.CommandText = query;
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("ModifiedDate", Convert.ToDateTime((DateTime.Now).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture)));
                        command.ExecuteNonQuery();
                    }
                    if (Errors.Count > 0)
                    {
                        odbcTransact.Rollback();
                        result.Error = Errors;
                    }
                    else
                    {
                        result.Status = 200;
                        result.Data = Wards;
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
        public APIResult GET(string WardID = null,
                            string CreatedFrom = null,
                            string CreatedTo = null,
                            string ModifiedFrom = null,
                            string ModifiedTo = null,
                            Nullable<bool> IsActive = null,
                            Nullable<int> PageSize = null,
                            Nullable<int> PageNum = null,
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
                string query = @"SELECT TOP " + _PageSize + @" START AT " + (_PageNum == 0 ? 1 : ((_PageNum * _PageSize) + 1)) + @" 
                                    W.WardID,
                                    W.WardNameEn,
                                    W.WardNameVn,
                                    W.IsActive,
                                    W.CreatedDate,
                                    W.ModifiedDate
                                    FROM DBA.Wards W
                                    WHERE W.WardID is not null";

                if (WardID != null)
                {
                    query += " AND WardID = '" + WardID + "'";
                }
                if (IsActive != null)
                {
                    query += " AND IsActive = " + (IsActive == true ? 1 : 0) + "";
                }
                if (CreatedFrom != null)
                {
                    DateTime Date = DateTime.Parse(CreatedFrom);
                    query += " AND CreatedDate >= '" + Date.ToString("yyyy/MM/dd 00:00:00", CultureInfo.InvariantCulture) + "'";
                }
                if (CreatedTo != null)
                {
                    DateTime Date = DateTime.Parse(CreatedTo);
                    query += " AND CreatedDate <= '" + Date.ToString("yyyy/MM/dd 23:59:59", CultureInfo.InvariantCulture) + "'";
                }
                if (ModifiedFrom != null)
                {
                    DateTime Date = DateTime.Parse(ModifiedFrom);
                    query += " AND ModifiedDate >= '" + Date.ToString("yyyy/MM/dd 00:00:00", CultureInfo.InvariantCulture) + "'";
                }
                if (ModifiedTo != null)
                {
                    DateTime Date = DateTime.Parse(ModifiedTo);
                    query += " AND ModifiedDate <= '" + Date.ToString("yyyy/MM/dd 23:59:59", CultureInfo.InvariantCulture) + "'";
                }
                string _OrderBy = "ASC";
                if (OrderBy == "DESC")
                {
                    _OrderBy = "DESC";
                }
                query += " ORDER BY CreatedDate " + _OrderBy + "";
                command.CommandText = query;
                DataTable Data = new DataTable("Wards");
                Data.Load(command.ExecuteReader());

                command.CommandText = @"SELECT COUNT(WardID)
                                        FROM dba.Wards
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
                result.Data = Data;
                result.TotalPages = TotalPages;
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
