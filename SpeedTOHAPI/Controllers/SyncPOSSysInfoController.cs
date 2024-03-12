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
    public class SyncPOSSysInfoController : ApiController
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

                List<POSSysInfoModel> SysInfos = JsonConvert.DeserializeObject<List<POSSysInfoModel>>(body);
                if (SysInfos.Count() < 0)
                {
                    result.Status = 643;
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

                    foreach (var sysinfo in SysInfos)
                    {
                        rowIndex++;
                        command.CommandText = @"SELECT COUNT(POSSysInfoID)
                                    FROM dba.POSSysInfo";
                        int CountSysInfo = (int)command.ExecuteScalar();
                        if (CountSysInfo > 0)
                        {
                            //UPDATE
                            try
                            {
                                command.CommandText = @"UPDATE dba.POSSysInfo
                                            SET Company = ?, TaxDes1 = ?,
                                            TaxDes2 = ?,TaxDes3 = ?,TaxDes4 = ?,TaxDes5 = ?,
                                            TaxRate1 = ?, TaxRate2 = ?, TaxRate3 = ?, TaxRate4 = ?, TaxRate5 = ?,
                                            TaxData1 = ?, TaxData2 = ?, TaxData3 = ?, TaxData4 = ?, TaxData5 = ?, UseVAT = ?";
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("Company", sysinfo.Company.ToString());
                                command.Parameters.AddWithValue("TaxDes1", sysinfo.TaxDes1.ToString());
                                command.Parameters.AddWithValue("TaxDes2", sysinfo.TaxDes2.ToString());
                                command.Parameters.AddWithValue("TaxDes3", sysinfo.TaxDes3.ToString());
                                command.Parameters.AddWithValue("TaxDes4", sysinfo.TaxDes4.ToString());
                                command.Parameters.AddWithValue("TaxDes5", sysinfo.TaxDes5.ToString());
                                command.Parameters.AddWithValue("TaxRate1", Convert.ToInt32(sysinfo.TaxRate1));
                                command.Parameters.AddWithValue("TaxRate2", Convert.ToInt32(sysinfo.TaxRate2));
                                command.Parameters.AddWithValue("TaxRate3", Convert.ToInt32(sysinfo.TaxRate3));
                                command.Parameters.AddWithValue("TaxRate4", Convert.ToInt32(sysinfo.TaxRate4));
                                command.Parameters.AddWithValue("TaxRate5", Convert.ToInt32(sysinfo.TaxRate5));
                                command.Parameters.AddWithValue("TaxData1", Convert.ToInt32(sysinfo.TaxData1));
                                command.Parameters.AddWithValue("TaxData2", Convert.ToInt32(sysinfo.TaxData2));
                                command.Parameters.AddWithValue("TaxData3", Convert.ToInt32(sysinfo.TaxData3));
                                command.Parameters.AddWithValue("TaxData4", Convert.ToInt32(sysinfo.TaxData4));
                                command.Parameters.AddWithValue("TaxData5", Convert.ToInt32(sysinfo.TaxData5));
                                command.Parameters.AddWithValue("UseVAT", Convert.ToInt32(sysinfo.UseVAT));
                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                var msg = new MessageModel { Status = 205, Function = "SyncPOSysInfo", Message = ex.Message, Description = ex.ToString() };
                                Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            }
                            continue;
                        }
                        try
                        {
                            command.CommandText = @"INSERT INTO dba.POSSysInfo
                                                (Company, TaxDes1,TaxDes2,TaxDes3,TaxDes4,TaxDes5,
                                                TaxRate1, TaxRate2, TaxRate3, TaxRate4, TaxRate5,
                                                TaxData1, TaxData2, TaxData3, TaxData4, TaxData5, UseVAT)
                                                VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("Company", sysinfo.Company.ToString());
                            command.Parameters.AddWithValue("TaxDes1", sysinfo.TaxDes1.ToString());
                            command.Parameters.AddWithValue("TaxDes2", sysinfo.TaxDes2.ToString());
                            command.Parameters.AddWithValue("TaxDes3", sysinfo.TaxDes3.ToString());
                            command.Parameters.AddWithValue("TaxDes4", sysinfo.TaxDes4.ToString());
                            command.Parameters.AddWithValue("TaxDes5", sysinfo.TaxDes5.ToString());
                            command.Parameters.AddWithValue("TaxRate1", Convert.ToInt32(sysinfo.TaxRate1));
                            command.Parameters.AddWithValue("TaxRate2", Convert.ToInt32(sysinfo.TaxRate2));
                            command.Parameters.AddWithValue("TaxRate3", Convert.ToInt32(sysinfo.TaxRate3));
                            command.Parameters.AddWithValue("TaxRate4", Convert.ToInt32(sysinfo.TaxRate4));
                            command.Parameters.AddWithValue("TaxRate5", Convert.ToInt32(sysinfo.TaxRate5));
                            command.Parameters.AddWithValue("TaxData1", Convert.ToInt32(sysinfo.TaxData1));
                            command.Parameters.AddWithValue("TaxData2", Convert.ToInt32(sysinfo.TaxData2));
                            command.Parameters.AddWithValue("TaxData3", Convert.ToInt32(sysinfo.TaxData3));
                            command.Parameters.AddWithValue("TaxData4", Convert.ToInt32(sysinfo.TaxData4));
                            command.Parameters.AddWithValue("TaxData5", Convert.ToInt32(sysinfo.TaxData5));
                            command.Parameters.AddWithValue("UseVAT", Convert.ToInt32(sysinfo.UseVAT));
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
                        result.Data = SysInfos;
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
