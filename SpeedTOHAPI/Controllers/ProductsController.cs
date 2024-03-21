using Newtonsoft.Json;
using SpeedTOHAPI.Codes;
using SpeedTOHAPI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace SpeedTOHAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ProductsController : ApiController
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

                List<ProductModel> Products = JsonConvert.DeserializeObject<List<ProductModel>>(body);
                if (Products.Count() < 0)
                {
                    result.Status = 2001;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }
                if (Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["MaxLengthAPI"]) < Products.Count())
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
                    result = GetCloud.GetStoreByGUID();
                    if (result.Status != 200)
                    {
                        var msg = new MessageModel { Status = 205, Function = "Categories", Message = result.Message, Description = result.Message };
                        result.Message = msg != null ? msg.Message : "";
                        return result;
                    }
                    List<CloudStoreModel> CloudStore = JsonConvert.DeserializeObject<List<CloudStoreModel>>(JsonConvert.SerializeObject(result.Data));
                    if (CloudStore.Count == 0)
                    {
                        result.Status = 209;
                        var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                        result.Message = msg != null ? msg.Message : "";
                        return result;
                    }
                    odbcTransact = conPixelSqlbase.BeginTransaction();
                    command.Transaction = odbcTransact;

                    foreach (var product in Products)
                    {
                        rowIndex++;
                        if (product.ProductName == null || product.ProductName == "")
                        {
                            var msg = Globals.GetStatusCode().Where(x => x.Status == 2002).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        if(product.SubCategoryCode == null)
                        {
                            var msg = Globals.GetStatusCode().Where(x => x.Status == 2003).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        else
                        {
                            command.CommandText = @"SELECT COUNT(SubCategoryID)
                                        FROM dba.SubCategories
                                        WHERE SubCategoryCode = '" + product.SubCategoryCode + @"'
                                          AND IsActive = 1";
                            int Count = (int)command.ExecuteScalar();

                            if (Count == 0)
                            {
                                result.Status = 2004;
                                var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                continue;
                            }
                        }
                        if (product.Price == null)
                        {
                            var msg = Globals.GetStatusCode().Where(x => x.Status == 2006).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        int ProductCode = 0;
                        CloudProductModel cloudProduct = new CloudProductModel();
                        try
                        {
                            cloudProduct.StoreID = CloudStore[0].StoreID;
                            cloudProduct.POSSubCategoryID = product.SubCategoryCode;
                            cloudProduct.PriceA = product.Price;
                            cloudProduct.PriceB = product.Price;
                            cloudProduct.PriceC = product.Price;
                            cloudProduct.PriceD = product.Price;
                            cloudProduct.PriceE = product.Price;
                            cloudProduct.PriceF = product.Price;
                            cloudProduct.PriceG = product.Price;
                            cloudProduct.PriceH = product.Price;
                            cloudProduct.PriceI = product.Price;
                            cloudProduct.PriceJ = product.Price;
                            cloudProduct.ModifyPrice = product.Price;
                            cloudProduct.ProductCode = -1;
                            cloudProduct.ProductName = product.ProductName;
                            result = PostProductCloud(cloudProduct);
                            if (result.Status != 200)
                            {
                                var msg = new MessageModel { Status = 205, Function = "Products", Message = result.Message, Description = result.Message };
                                Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                continue;
                            }
                            CloudProductModel CloudProduct = JsonConvert.DeserializeObject<CloudProductModel>(JsonConvert.SerializeObject(result.Data));
                            ProductCode = CloudProduct.ProductCode;
                            command.CommandText = @"INSERT INTO dba.Products
                                                (ProductCode,ProductName, SubCategoryCode,Price,
                                                Tax1,Tax2,Tax3,Tax4,Tax5,
                                                TimeStartOrder, TimeEndOrder, Kcal)
                                                VALUES(?,?,?,?,?,?,?,?,?,?,?,?)";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("ProductCode", Convert.ToInt32(ProductCode));
                            command.Parameters.AddWithValue("ProductName", product.ProductName);
                            command.Parameters.AddWithValue("SubCategoryCode", product.SubCategoryCode);
                            command.Parameters.AddWithValue("Price", product.Price);
                            command.Parameters.AddWithValue("Tax1", product.Tax1 != null ? product.Tax1 : null);
                            command.Parameters.AddWithValue("Tax2", product.Tax2 != null ? product.Tax2 : null);
                            command.Parameters.AddWithValue("Tax3", product.Tax3 != null ? product.Tax3 : null);
                            command.Parameters.AddWithValue("Tax4", product.Tax4 != null ? product.Tax4 : null);
                            command.Parameters.AddWithValue("Tax5", product.Tax5 != null ? product.Tax5 : null);
                            command.Parameters.AddWithValue("TimeStartOrder", product.TimeStartOrder != null ? product.TimeStartOrder : null);
                            command.Parameters.AddWithValue("TimeEndOrder", product.TimeEndOrder != null ? product.TimeEndOrder : null);
                            command.Parameters.AddWithValue("Kcal", product.Kcal != null ? product.Kcal : null);
                            command.ExecuteNonQuery();
  
                        }
                        catch (Exception ex)
                        {
                            if(ProductCode != 0)
                            {
                                cloudProduct.ProductCode = ProductCode;
                                cloudProduct.IsActive = false;
                                PostProductCloud(cloudProduct);
                            }
                            var msg = new MessageModel { Status = 205, Function = "Products", Message = ex.Message, Description = ex.ToString() };
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }

                    }
                    if (Errors.Count > 0)
                    {
                        odbcTransact.Rollback();
                        result.Message = "";
                        result.Data = null;
                        result.Status = 205;
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

                List<ProductModel> Products = JsonConvert.DeserializeObject<List<ProductModel>>(body);
                if (Products.Count() < 0)
                {
                    result.Status = 2001;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }
                if (Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["MaxLengthAPI"]) < Products.Count())
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
                    result = GetCloud.GetStoreByGUID();
                    if (result.Status != 200)
                    {
                        var msg = new MessageModel { Status = 205, Function = "SubCategories", Message = result.Message, Description = result.Message };
                        result.Message = msg != null ? msg.Message : "";
                        return result;
                    }
                    List<CloudStoreModel> cloudStores = JsonConvert.DeserializeObject<List<CloudStoreModel>>(JsonConvert.SerializeObject(result.Data));
                    if (cloudStores.Count == 0)
                    {
                        result.Status = 209;
                        var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                        result.Message = msg != null ? msg.Message : "";
                        return result;
                    }
                    odbcTransact = conPixelSqlbase.BeginTransaction();
                    command.Transaction = odbcTransact;

                    foreach (var product in Products)
                    {
                        rowIndex++;
                        try
                        {
                            if (product.ProductCode == null)
                            {
                                result.Status = 2005;
                                var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                continue;
                            }
                            command.CommandText = @"SELECT COUNT(ProductCode)
                                        FROM dba.Products
                                        WHERE ProductCode = '" + product.ProductCode + @"'
                                          AND IsActive = 1";
                            int Count = (int)command.ExecuteScalar();

                            if (Count == 0)
                            {
                                result.Status = 2005;
                                var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                continue;
                            }

                            CloudProductModel cloudProduct = new CloudProductModel();

                            string query = @"UPDATE dba.Products
                                            SET DateModified = ?";
                            cloudProduct.ProductCode = (int)product.ProductCode;
                            cloudProduct.StoreID = cloudStores[0].StoreID;

                            if (product.ProductName != null && product.ProductName != "")
                            {
                                cloudProduct.ProductName = product.ProductName;
                                query += ", ProductName = '" + product.ProductName + "'";
                            }
                            if (product.SubCategoryCode != null)
                            {
                                cloudProduct.POSSubCategoryID = product.SubCategoryCode;
                                query += ", SubCategoryCode = '" + product.SubCategoryCode + "'";
                            }
                            if (product.Price != null)
                            {
                                cloudProduct.PriceA = product.Price;
                                cloudProduct.PriceB = product.Price;
                                cloudProduct.PriceC = product.Price;
                                cloudProduct.PriceD = product.Price;
                                cloudProduct.PriceE = product.Price;
                                cloudProduct.PriceF = product.Price;
                                cloudProduct.PriceG = product.Price;
                                cloudProduct.PriceH = product.Price;
                                cloudProduct.PriceI = product.Price;
                                cloudProduct.PriceJ = product.Price;
                                cloudProduct.ModifyPrice = product.Price;
                                query += ", Price = '" + product.Price + "'";
                            }
                            if (product.Tax1 != null)
                            {
                                cloudProduct.Tax1 = Convert.ToInt32(product.Tax1);
                                query += ", Tax1 = '" + product.Tax1 + "'";
                            }
                            if (product.Tax2 != null)
                            {
                                cloudProduct.Tax2 = Convert.ToInt32(product.Tax2);
                                query += ", Tax2 = '" + product.Tax2 + "'";
                            }
                            if (product.Tax3 != null)
                            {
                                cloudProduct.Tax3 = Convert.ToInt32(product.Tax3);
                                query += ", Tax3 = '" + product.Tax3 + "'";
                            }
                            if (product.Tax4 != null)
                            {
                                cloudProduct.Tax4 = Convert.ToInt32(product.Tax4);
                                query += ", Tax4 = '" + product.Tax4 + "'";
                            }
                            if (product.Tax5 != null)
                            {
                                cloudProduct.Tax5 = Convert.ToInt32(product.Tax5);
                                query += ", Tax5 = '" + product.Tax5 + "'";
                            }
                            if (product.Question1 != null)
                            {
                                cloudProduct.QuestionID1 = Convert.ToInt32(product.Question1);
                                query += ", Question1 = '" + product.Question1 + "'";
                            }
                            if (product.Question2 != null)
                            {
                                cloudProduct.QuestionID2 = Convert.ToInt32(product.Question2);
                                query += ", Question2 = '" + product.Question2 + "'";
                            }
                            if (product.Question2 != null)
                            {
                                cloudProduct.QuestionID3 = Convert.ToInt32(product.Question3);
                                query += ", Question3 = '" + product.Question3 + "'";
                            }
                            if (product.Question4 != null)
                            {
                                cloudProduct.QuestionID4 = Convert.ToInt32(product.Question4);
                                query += ", Question4 = '" + product.Question4 + "'";
                            }
                            if (product.Question5 != null)
                            {
                                cloudProduct.QuestionID5 = Convert.ToInt32(product.Question5);
                                query += ", Question5 = '" + product.Question5 + "'";
                            }
                            if (product.IsPublic != null)
                            {
                                query += ", IsPublic = '" + product.IsPublic + "'";
                            }
                            if (product.Index != null)
                            {
                                query += ", Index = '" + product.Index + "'";
                            }
                            if (product.IsActive != null)
                            {
                                query += ", IsActive = '" + product.IsActive + "'";
                            }
                            if (product.Image != null)
                            {
                                string img = SaveImage(product.Image, product.ProductCode.ToString());
                                query += ", Image = '" + img + "'";
                            }
                            if (product.TimeStartOrder != null)
                            {
                                query += ", TimeStartOrder = '" + product.TimeStartOrder + "'";
                            }
                            if (product.TimeEndOrder != null)
                            {
                                query += ", TimeEndOrder = '" + product.TimeEndOrder + "'";
                            }
                            if (product.Kcal != null)
                            {
                                query += ", Kcal = '" + product.Kcal + "'";
                            }
                            result = PostProductCloud(cloudProduct);
                            if (result.Status != 200)
                            {
                                var msg = new MessageModel { Status = 205, Function = "SubCategories", Message = result.Message, Description = result.Message };
                                Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                continue;
                            }
                            query += " WHERE ProductCode=" + (int)product.ProductCode + "";
                            command.CommandText = query;
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("DateModified", Convert.ToDateTime((DateTime.Now).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture)));
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            var msg = new MessageModel { Status = 205, Function = "Categories", Message = ex.Message, Description = ex.ToString() };
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }

                    }
                    if (Errors.Count > 0)
                    {
                        odbcTransact.Rollback();
                        result.Status = 205;
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
        [HttpGet]
        public APIResult GET(string ProductCode = null,
            string SubCategoryCode = null,
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
                                    *
                                    FROM DBA.Products R
                                    WHERE R.ProductID is not null";
                if (ProductCode != null)
                {
                    query += " AND ProductCode = '" + ProductCode + "'";
                }
                if (SubCategoryCode != null)
                {
                    query += " AND SubCategoryCode = '" + SubCategoryCode + "'";
                }
                if (IsActive != null)
                {
                    query += " AND IsActive = " + (IsActive == true ? 1 : 0) + "";
                }
                if (CreatedFrom != null)
                {
                    DateTime Date = DateTime.Parse(CreatedFrom);
                    query += " AND DateCreated >= '" + Date.ToString("yyyy/MM/dd 00:00:00", CultureInfo.InvariantCulture) + "'";
                }
                if (CreatedTo != null)
                {
                    DateTime Date = DateTime.Parse(CreatedTo);
                    query += " AND DateCreated <= '" + Date.ToString("yyyy/MM/dd 23:59:59", CultureInfo.InvariantCulture) + "'";
                }
                if (ModifiedFrom != null)
                {
                    DateTime Date = DateTime.Parse(ModifiedFrom);
                    query += " AND DateModified >= '" + Date.ToString("yyyy/MM/dd 00:00:00", CultureInfo.InvariantCulture) + "'";
                }
                if (ModifiedTo != null)
                {
                    DateTime Date = DateTime.Parse(ModifiedTo);
                    query += " AND DateModified <= '" + Date.ToString("yyyy/MM/dd 23:59:59", CultureInfo.InvariantCulture) + "'";
                }
                string _OrderBy = "ASC";
                if (OrderBy == "DESC")
                {
                    _OrderBy = "DESC";
                }
                query += " ORDER BY ProductID " + _OrderBy + "";
                command.CommandText = query;
                DataTable Data = new DataTable("Products");
                Data.Load(command.ExecuteReader());

                command.CommandText = @"SELECT COUNT(ProductID)
                                        FROM dba.Products
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
        private APIResult PostProductCloud(CloudProductModel cloudProduct)
        {
            APIResult result = new APIResult();
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(System.Configuration.ConfigurationManager.AppSettings["ServerTickX"].ToString() + "/api/Product");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.Timeout = 100000;
            httpWebRequest.Headers.Add("PartnerGUID", System.Configuration.ConfigurationManager.AppSettings["PartnerKeyTickX"].ToString());
            httpWebRequest.Headers.Add("Token", System.Configuration.ConfigurationManager.AppSettings["TokenTickX"].ToString());
            httpWebRequest.Headers.Add("CurrentUTC", System.Configuration.ConfigurationManager.AppSettings["CurrentUTCTickX"].ToString());
            httpWebRequest.Headers.Add("ClientGUID", System.Configuration.ConfigurationManager.AppSettings["ClientGUIDTickX"].ToString());
            string strBody = Newtonsoft.Json.JsonConvert.SerializeObject(cloudProduct);

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
        public string SaveImage(string ImgStr, string ImgName)
        {
            String path = HttpContext.Current.Server.MapPath("~/Images/Products"); //Path

            //Check if directory exist
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path); //Create directory if it doesn't exist
            }

            string imageName = ImgName + ".jpg";

            //set the image path
            string imgPath = Path.Combine(path, imageName);

            byte[] imageBytes = Convert.FromBase64String(ImgStr);

            File.WriteAllBytes(imgPath, imageBytes);
            string imageUrl = "/Images/Products/" + imageName;
            return imageUrl;
        }
    }
}
