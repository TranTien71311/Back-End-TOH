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
using System.Web.Http;
using System.Web.Http.Cors;

namespace SpeedTOHAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class CategoriesController : ApiController
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

                List<CategoryModel> Categories = JsonConvert.DeserializeObject<List<CategoryModel>>(body);
                if (Categories.Count() < 0)
                {
                    result.Status = 1801;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }
                if (Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["MaxLengthAPI"]) < Categories.Count())
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
                    List<CloudStoreModel> cloudStores = JsonConvert.DeserializeObject<List<CloudStoreModel>>(JsonConvert.SerializeObject(result.Data));
                    if(cloudStores.Count == 0)
                    {
                        result.Status = 209;
                        var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                        result.Message = msg != null ? msg.Message : "";
                        return result;
                    }
                    odbcTransact = conPixelSqlbase.BeginTransaction();
                    command.Transaction = odbcTransact;

                    foreach (var cat in Categories)
                    {
                        rowIndex++;
                        if(cat.CategoryName == null || cat.CategoryName == "")
                        {
                            var msg = Globals.GetStatusCode().Where(x => x.Status == 1802).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        try
                        {
                            result = GetCategoryMax();
                            if(result.Status != 200)
                            {
                                
                                var msg = new MessageModel { Status = 205, Function = "Categories", Message = result.Message, Description = result.Message };
                                Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                continue;
                            }
                            List<CloudPOSCategoryModel> CloudPOSCategory = JsonConvert.DeserializeObject<List<CloudPOSCategoryModel>>(JsonConvert.SerializeObject(result.Data));
                            int CloudID = CloudPOSCategory[0].POSCategoryID;
                            int CategoryCode = 0;
                            if(CloudID > 1000000000)
                            {
                                CategoryCode = CloudID + 1;
                            }
                            else
                            {
                                CategoryCode = 1000000000 + CloudID; 
                            }
                            command.CommandText = @"INSERT INTO dba.Categories
                                                (CategoryCode, CategoryName)
                                                VALUES(?,?)";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("CategoryCode", Convert.ToInt32(CategoryCode));
                            command.Parameters.AddWithValue("ReportName", cat.CategoryName);
                            command.ExecuteNonQuery();

                            CloudPOSCategoryModel cloudPOSCategory = new CloudPOSCategoryModel();
                            cloudPOSCategory.POSCategoryID = CategoryCode;
                            cloudPOSCategory.CategoryName = cat.CategoryName;
                            cloudPOSCategory.StoreID = cloudStores[0].StoreID;
                            
                            result = PostCategory(cloudPOSCategory);
                            if (result.Status != 200)
                            {
                                var msg = new MessageModel { Status = 205, Function = "Categories", Message = result.Message, Description = result.Message };
                                Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                continue;
                            }
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
                        result.Data = Categories;
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

                List<CategoryModel> Categories = JsonConvert.DeserializeObject<List<CategoryModel>>(body);
                if (Categories.Count() < 0)
                {
                    result.Status = 1801;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }
                if (Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["MaxLengthAPI"]) < Categories.Count())
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

                    foreach (var cat in Categories)
                    {
                        rowIndex++;
                        try
                        {
                            command.CommandText = @"SELECT COUNT(CategoryID)
                                        FROM dba.Categories
                                        WHERE CategoryCode = '" + cat.CategoryCode + @"'
                                          AND IsActive = 1";
                            int Count = (int)command.ExecuteScalar();

                            if (Count == 0)
                            {
                                result.Status = 1803;
                                var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                continue;
                            }

                            CloudPOSCategoryModel cloudPOSCategory = new CloudPOSCategoryModel();
                            
                            string query = @"UPDATE dba.Categories
                                            SET ModifiedDate = ?";
                            cloudPOSCategory.POSCategoryID = (int)cat.CategoryCode;
                            cloudPOSCategory.StoreID = cloudStores[0].StoreID;

                            if (cat.CategoryName != null && cat.CategoryName != "")
                            {
                                cloudPOSCategory.CategoryName = cat.CategoryName;
                                query += ", CategoryName = '" + cat.CategoryName + "'";
                            }
                            result = PostCategory(cloudPOSCategory);
                            if (result.Status != 200)
                            {
                                var msg = new MessageModel { Status = 205, Function = "Categories", Message = result.Message, Description = result.Message };
                                Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                continue;
                            }
                            query += " WHERE CategoryCode=" + (int)cat.CategoryCode + "";
                            command.CommandText = query;
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("ModifiedDate", Convert.ToDateTime((DateTime.Now).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture)));
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
                        result.Data = Categories;
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
        public APIResult GET(string CategoryID = null,
                    string CategoryCode = null,
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
                                    R.CategoryID,
                                    R.CategoryCode,
                                    R.CategoryName,
                                    R.IsActive,
                                    R.CreatedDate,
                                    R.ModifiedDate,
                                    R.IsSync
                                    FROM DBA.Categories R
                                    WHERE R.CategoryID is not null";
                if (CategoryID != null)
                {
                    query += " AND CategoryID = '" + CategoryID + "'";
                }
                if (CategoryCode != null)
                {
                    query += " AND CategoryCode = '" + CategoryCode + "'";
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
                query += " ORDER BY CategoryID " + _OrderBy + "";
                command.CommandText = query;
                DataTable Data = new DataTable("Categories");
                Data.Load(command.ExecuteReader());

                command.CommandText = @"SELECT COUNT(CategoryID)
                                        FROM dba.Categories
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
        private APIResult GetCategoryMax()
        {
            APIResult result = new APIResult();
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(System.Configuration.ConfigurationManager.AppSettings["ServerTickX"].ToString() + "/api/POSCategory?PageSize=1000&PageNum=0&OrderBy=DESC");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";
            httpWebRequest.Timeout = 100000;
            httpWebRequest.Headers.Add("PartnerGUID", System.Configuration.ConfigurationManager.AppSettings["PartnerKeyTickX"].ToString());
            httpWebRequest.Headers.Add("Token", System.Configuration.ConfigurationManager.AppSettings["TokenTickX"].ToString());
            httpWebRequest.Headers.Add("CurrentUTC", System.Configuration.ConfigurationManager.AppSettings["CurrentUTCTickX"].ToString());
            httpWebRequest.Headers.Add("ClientGUID", System.Configuration.ConfigurationManager.AppSettings["ClientGUIDTickX"].ToString());

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var srPayPos = new StreamReader(httpResponse.GetResponseStream()))
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<APIResult>(srPayPos.ReadToEnd());
            }
        }
        private APIResult PostCategory(CloudPOSCategoryModel cloudPOSCategory)
        {
            APIResult result = new APIResult();
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(System.Configuration.ConfigurationManager.AppSettings["ServerTickX"].ToString() + "/api/POSCategory");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.Timeout = 100000;
            httpWebRequest.Headers.Add("PartnerGUID", System.Configuration.ConfigurationManager.AppSettings["PartnerKeyTickX"].ToString());
            httpWebRequest.Headers.Add("Token", System.Configuration.ConfigurationManager.AppSettings["TokenTickX"].ToString());
            httpWebRequest.Headers.Add("CurrentUTC", System.Configuration.ConfigurationManager.AppSettings["CurrentUTCTickX"].ToString());
            httpWebRequest.Headers.Add("ClientGUID", System.Configuration.ConfigurationManager.AppSettings["ClientGUIDTickX"].ToString());
            string strBody = Newtonsoft.Json.JsonConvert.SerializeObject(cloudPOSCategory);

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
