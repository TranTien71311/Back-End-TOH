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
    public class SyncPOSProductsController : ApiController
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

                List<POSProductModel> Products = JsonConvert.DeserializeObject<List<POSProductModel>>(body);
                if (Products.Count() < 0)
                {
                    result.Status = 601;
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

                    foreach (var product in Products)
                    {
                        rowIndex++;
                        if(product.ProductNum == null)
                        {
                            result.Status = 602;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        else
                        {
                            command.CommandText = @"SELECT COUNT(ProductNum)
                                        FROM dba.POSProducts
                                        WHERE ProductNum = '" + product.ProductNum + @"'";
                            int CountProductNum = (int)command.ExecuteScalar();
                            if(CountProductNum > 0)
                            {
                                try 
                                {
                                    command.CommandText = @"UPDATE dba.POSProducts
                                                SET ProductName = ?, ReportNo = ?,
                                                PriceA = ?,PriceB = ?,PriceC = ?,PriceD = ?,PriceE = ?,PriceF = ?,PriceG = ?,PriceH = ?,PriceI = ?,PriceJ = ?,
                                                Tax1 = ?,Tax2 = ?,Tax3 = ?,Tax4 = ?,Tax5 = ?,
                                                Question1 = ?,Question2 = ?,Question3 = ?,Question4 = ?,Question5 = ?,
                                                ProductType= ?,SizeUp = ?,SizeDown = ?,LabelCapacity = ?, IsActive = ?, DateModified = ?
                                                WHERE ProductNum = ?";
                                    command.Parameters.Clear();
                                    command.Parameters.AddWithValue("ProductName", product.ProductName.ToString());
                                    command.Parameters.AddWithValue("ReportNo", Convert.ToInt32(product.ReportNo));
                                    command.Parameters.AddWithValue("PriceA", Convert.ToDouble(product.PriceA));
                                    command.Parameters.AddWithValue("PriceB", Convert.ToDouble(product.PriceB));
                                    command.Parameters.AddWithValue("PriceC", Convert.ToDouble(product.PriceC));
                                    command.Parameters.AddWithValue("PriceD", Convert.ToDouble(product.PriceD));
                                    command.Parameters.AddWithValue("PriceE", Convert.ToDouble(product.PriceE));
                                    command.Parameters.AddWithValue("PriceF", Convert.ToDouble(product.PriceF));
                                    command.Parameters.AddWithValue("PriceG", Convert.ToDouble(product.PriceG));
                                    command.Parameters.AddWithValue("PriceH", Convert.ToDouble(product.PriceH));
                                    command.Parameters.AddWithValue("PriceI", Convert.ToDouble(product.PriceI));
                                    command.Parameters.AddWithValue("PriceJ", Convert.ToDouble(product.PriceJ));
                                    command.Parameters.AddWithValue("Tax1", Convert.ToBoolean(product.Tax1));
                                    command.Parameters.AddWithValue("Tax2", Convert.ToBoolean(product.Tax2));
                                    command.Parameters.AddWithValue("Tax3", Convert.ToBoolean(product.Tax3));
                                    command.Parameters.AddWithValue("Tax4", Convert.ToBoolean(product.Tax4));
                                    command.Parameters.AddWithValue("Tax5", Convert.ToBoolean(product.Tax5));
                                    command.Parameters.AddWithValue("Question1", Convert.ToInt32(product.Question1));
                                    command.Parameters.AddWithValue("Question2", Convert.ToInt32(product.Question2));
                                    command.Parameters.AddWithValue("Question3", Convert.ToInt32(product.Question3));
                                    command.Parameters.AddWithValue("Question4", Convert.ToInt32(product.Question4));
                                    command.Parameters.AddWithValue("Question5", Convert.ToInt32(product.Question5));
                                    command.Parameters.AddWithValue("ProductType", Convert.ToInt32(product.ProductType));
                                    command.Parameters.AddWithValue("SizeUp", Convert.ToBoolean(product.SizeUp));
                                    command.Parameters.AddWithValue("SizeDown", Convert.ToBoolean(product.SizeDown));
                                    if (product.LabelCapacity != null)
                                    {
                                        command.Parameters.AddWithValue("LabelCapacity", product.LabelCapacity.ToString());
                                    }
                                    else
                                    {
                                        command.Parameters.AddWithValue("LabelCapacity", DBNull.Value);
                                    }
                                    command.Parameters.AddWithValue("IsActive", Convert.ToBoolean(product.IsActive));
                                    command.Parameters.AddWithValue("DateModified", Convert.ToDateTime((DateTime.Now).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture)));
                                    command.Parameters.AddWithValue("ProductNum", Convert.ToInt32(product.ProductNum));

                                    command.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    var msg = new MessageModel { Status = 205, Function = "SyncPOSProduct", Message = ex.Message, Description = ex.ToString() };
                                    Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                }
                                continue;
                            }
                        }
                        try
                        {
                            command.CommandText = @"INSERT INTO dba.POSProducts
                                                (ProductNum, ProductName, ReportNo,
                                                PriceA,PriceB,PriceC,PriceD,PriceE,PriceF,PriceG,PriceH,PriceI,PriceJ,
                                                Tax1,Tax2,Tax3,Tax4,Tax5,
                                                Question1,Question2,Question3,Question4,Question5,
                                                ProductType,SizeUp,SizeDown,LabelCapacity,IsActive)
                                                VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("ProductNum", Convert.ToInt32(product.ProductNum));
                            command.Parameters.AddWithValue("ProductName", product.ProductName.ToString());
                            command.Parameters.AddWithValue("ReportNo", Convert.ToInt32(product.ReportNo));
                            command.Parameters.AddWithValue("PriceA", Convert.ToDouble(product.PriceA));
                            command.Parameters.AddWithValue("PriceB", Convert.ToDouble(product.PriceB));
                            command.Parameters.AddWithValue("PriceC", Convert.ToDouble(product.PriceC));
                            command.Parameters.AddWithValue("PriceD", Convert.ToDouble(product.PriceD));
                            command.Parameters.AddWithValue("PriceE", Convert.ToDouble(product.PriceE));
                            command.Parameters.AddWithValue("PriceF", Convert.ToDouble(product.PriceF));
                            command.Parameters.AddWithValue("PriceG", Convert.ToDouble(product.PriceG));
                            command.Parameters.AddWithValue("PriceH", Convert.ToDouble(product.PriceH));
                            command.Parameters.AddWithValue("PriceI", Convert.ToDouble(product.PriceI));
                            command.Parameters.AddWithValue("PriceJ", Convert.ToDouble(product.PriceJ));
                            command.Parameters.AddWithValue("Tax1", Convert.ToBoolean(product.Tax1));
                            command.Parameters.AddWithValue("Tax2", Convert.ToBoolean(product.Tax2));
                            command.Parameters.AddWithValue("Tax3", Convert.ToBoolean(product.Tax3));
                            command.Parameters.AddWithValue("Tax4", Convert.ToBoolean(product.Tax4));
                            command.Parameters.AddWithValue("Tax5", Convert.ToBoolean(product.Tax5));
                            command.Parameters.AddWithValue("Question1", Convert.ToInt32(product.Question1));
                            command.Parameters.AddWithValue("Question2", Convert.ToInt32(product.Question2));
                            command.Parameters.AddWithValue("Question3", Convert.ToInt32(product.Question3));
                            command.Parameters.AddWithValue("Question4", Convert.ToInt32(product.Question4));
                            command.Parameters.AddWithValue("Question5", Convert.ToInt32(product.Question5));
                            command.Parameters.AddWithValue("ProductType", Convert.ToInt32(product.ProductType));
                            command.Parameters.AddWithValue("SizeUp", Convert.ToBoolean(product.SizeUp));
                            command.Parameters.AddWithValue("SizeDown", Convert.ToBoolean(product.SizeDown));
                            if (product.LabelCapacity != null)
                            {
                                command.Parameters.AddWithValue("LabelCapacity", product.LabelCapacity.ToString());
                            }
                            else
                            {
                                command.Parameters.AddWithValue("LabelCapacity", DBNull.Value);
                            }
                            command.Parameters.AddWithValue("IsActive", Convert.ToBoolean(product.IsActive));
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            var msg = new MessageModel { Status = 205, Function = "SyncPOSProduct", Message = ex.Message, Description = ex.ToString() };
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
                        result.Data = Products;
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
