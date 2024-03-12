using Newtonsoft.Json;
using SpeedTOHAPI.Codes;
using SpeedTOHAPI.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Odbc;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace SpeedTOHAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class OrderAlacarteController : ApiController
    {
       [HttpPost]
       public APIResult Post([NakedBody] string body)
        {
            APIResult result = new APIResult();
            OdbcConnection conPixelSqlbase = new OdbcConnection();

            OdbcTransaction odbcTransact = null;
            
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
                OdbcCommand command = new OdbcCommand();
                conPixelSqlbase.Open();

                command.Connection = conPixelSqlbase;

                odbcTransact = conPixelSqlbase.BeginTransaction();
                command.Transaction = odbcTransact;
                OrderAlacarteModel OrderAlacarte = JsonConvert.DeserializeObject<OrderAlacarteModel>(body);
                if(OrderAlacarte != null)
                {

                    if (OrderAlacarte.POSHeader != null && OrderAlacarte.POSDetails != null && OrderAlacarte.POSDetails.Count() != 0)
                    {
                        result = PostHeader(OrderAlacarte.POSHeader);
                        if (result.Status == 200)
                        {
                            List<POSHeaderModel> pOSHeaders = JsonConvert.DeserializeObject<List<POSHeaderModel>>(JsonConvert.SerializeObject(result.Data));
                            List<POSDetailModel> pOSDetails = new List<POSDetailModel>();
                            foreach(var detail in OrderAlacarte.POSDetails)
                            {
                                detail.TRANSACT = pOSHeaders[0].TRANSACT;
                                pOSDetails.Add(detail);
                            }
                            result = PostDetails(pOSDetails);
                            if(result.Status == 200)
                            {
                                //result = PostMsg(OrderAlacarte.Msg);
                                //if(result.Status == 200)
                                //{
                                    //Insert Alacarte
                                    command.CommandText = @"INSERT INTO DBA.TransactionAlacartes (TransactionCode, OpenDate, UserOrder, PatientID)
                                                                    VALUES (?,?,?,?)";
                                    command.Parameters.Clear();
                                    command.Parameters.AddWithValue("TransactionCode", Convert.ToInt32(pOSHeaders[0].TRANSACT));
                                    command.Parameters.AddWithValue("OpenDate", Convert.ToDateTime(pOSHeaders[0].OPENDATE));
                                    command.Parameters.AddWithValue("UserOrder", Convert.ToInt32(OrderAlacarte.Transaction.UserOrder));
                                    command.Parameters.AddWithValue("PatientID", Convert.ToInt32(OrderAlacarte.Transaction.PatientID));
                                    command.ExecuteNonQuery();

                                    //Insert Alacarte Detail
                                    foreach (var detail in OrderAlacarte.POSDetails)
                                    {
                                        command.CommandText = @"INSERT INTO DBA.TransactionDetailAlacartes (TransactionCode, ProductNum, Quantity, Price, TotalTax)
                                                                    VALUES (?,?,?,?,?)";
                                        command.Parameters.Clear();
                                        command.Parameters.AddWithValue("TransactionCode", Convert.ToInt32(pOSHeaders[0].TRANSACT));
                                        command.Parameters.AddWithValue("ProductNum", Convert.ToInt32(detail.PRODNUM));
                                        command.Parameters.AddWithValue("Quantity", Convert.ToInt32(detail.QUAN));
                                        command.Parameters.AddWithValue("Price", Convert.ToDouble(detail.COSTEACH));
                                        command.Parameters.AddWithValue("TotalTax", Convert.ToDouble(detail.NetCostEach - detail.COSTEACH));
                                        command.ExecuteNonQuery();
                                    }


                                    odbcTransact.Commit();
                                //}
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if(odbcTransact != null)
                {
                    odbcTransact.Rollback();
                }
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

        private APIResult PostHeader(POSHeaderModel _POSHeader)
        {
            APIResult result = new APIResult();
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(System.Configuration.ConfigurationManager.AppSettings["ServerAPI"].ToString() + "/api/POSHeader");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.Timeout = 100000;
            httpWebRequest.Headers.Add("PartnerKey", System.Configuration.ConfigurationManager.AppSettings["PartnerKeyAPI"].ToString());
            string strBody = Newtonsoft.Json.JsonConvert.SerializeObject(_POSHeader);
            httpWebRequest.Headers.Add("Token", Globals.HMACSHA256("/api/POSHeader" + "." + strBody, System.Configuration.ConfigurationManager.AppSettings["SecretKeyAPI"].ToString()));

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(strBody);
            }
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var srPayPos = new StreamReader(httpResponse.GetResponseStream()))
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<APIResult>(srPayPos.ReadToEnd());
            }
        }
        private APIResult PostDetails(List<POSDetailModel> _POSDetails)
        {
            APIResult result = new APIResult();
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(System.Configuration.ConfigurationManager.AppSettings["ServerAPI"].ToString() + "/api/POSDetail");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.Timeout = 100000;
            httpWebRequest.Headers.Add("PartnerKey", System.Configuration.ConfigurationManager.AppSettings["PartnerKeyAPI"].ToString());
            string strBody = Newtonsoft.Json.JsonConvert.SerializeObject(_POSDetails);
            httpWebRequest.Headers.Add("Token", Globals.HMACSHA256("/api/POSDetail" + "." + strBody, System.Configuration.ConfigurationManager.AppSettings["SecretKeyAPI"].ToString()));

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(strBody);
            }
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var srPayPos = new StreamReader(httpResponse.GetResponseStream()))
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<APIResult>(srPayPos.ReadToEnd());
            }
        }

        private APIResult PostMsg(MsgModel msg)
        {
            APIResult result = new APIResult();
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(System.Configuration.ConfigurationManager.AppSettings["ServerAPI"].ToString() + "/api/POSHeader");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.Timeout = 100000;
            httpWebRequest.Headers.Add("PartnerKey", System.Configuration.ConfigurationManager.AppSettings["PartnerKeyAPI"].ToString());
            string strBody = Newtonsoft.Json.JsonConvert.SerializeObject(msg);
            httpWebRequest.Headers.Add("Token", Globals.HMACSHA256("/api/MsgMgr" + "." + strBody, System.Configuration.ConfigurationManager.AppSettings["SecretKeyAPI"].ToString()));

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(strBody);
            }
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var srPayPos = new StreamReader(httpResponse.GetResponseStream()))
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<APIResult>(srPayPos.ReadToEnd());
            }
        }
    }
}
