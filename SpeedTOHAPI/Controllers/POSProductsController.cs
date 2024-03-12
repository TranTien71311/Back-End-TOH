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
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Util;

namespace SpeedTOHAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class POSProductsController : ApiController
    {
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

                List<POSProductModel> Products = JsonConvert.DeserializeObject<List<POSProductModel>>(body);
                if (Products.Count() < 0)
                {
                    result.Status = 601;
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
                    odbcTransact = conPixelSqlbase.BeginTransaction();
                    command.Transaction = odbcTransact;

                    foreach (var product in Products)
                    {
                        rowIndex++;
                        if (product.ProductNum == null)
                        {
                            result.Status = 602;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        else
                        {
                            command.CommandText = @"SELECT ISNULL(ProductNum, -1)
                                        FROM dba.POSProducts
                                        WHERE ProductNum = '" + product.ProductNum + @"'";
                            int ProductNum = (int)command.ExecuteScalar();
                            if (ProductNum == -1)
                            {
                                result.Status = 611;
                                var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                continue;
                            }
                            //UPDATE
                            try
                            {
                                string query = "UPDATE DBA.POSProducts SET DateModified= ? ";
                                if(product.IsPublic != null)
                                {
                                    query += ", IsPublic='" + Convert.ToInt32(product.IsPublic) + "'";
                                }
                                if (product.Image != null && product.Image != "")
                                {
                                    string img = SaveImage(product.Image, product.ProductNum.ToString());
                                    Random rnd = new Random();
                                    Products[0].Image = img + "?" + rnd.Next();
                                    query += ", Image = '" + img + "'";
                                }
                                if (product.Index != null)
                                {
                                    query += @", ""Index"" = '" + product.Index + "'";
                                }
                                if (product.TimeStartOrder != null)
                                {
                                    query += @", TimeStartOrder = '" + product.TimeStartOrder + "'";
                                }
                                if (product.TimeEndOrder != null)
                                {
                                    query += @", TimeEndOrder = '" + product.TimeEndOrder + "'";
                                }
                                query += " WHERE ProductNum='" + product.ProductNum + "'";

                                command.CommandText = query;
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("DateModified", Convert.ToDateTime((DateTime.Now).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture)));
                                command.ExecuteNonQuery();

                                if (product.Translations != null && product.Translations.Count() > 0)
                                {
                                    foreach(var translation  in product.Translations)
                                    {
                                        if (translation.TranslationType == null)
                                        {
                                            result.Status = 613;
                                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                            continue;
                                        }
                                        if (translation.TranslationText == null || translation.TranslationText == "")
                                        {
                                            result.Status = 614;
                                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                            continue;
                                        }
                                        command.CommandText = @"SELECT ISNULL(TranslationID, -1)
                                        FROM dba.Translations
                                        WHERE TranslationID = '" + translation.TranslationID + @"'";
                                        int TranslationID = (int)command.ExecuteScalar();
                                        if (TranslationID == -1)
                                        {
                                            result.Status = 612;
                                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                            continue;
                                        }
                                        command.CommandText = @"SELECT COUNT(TranslationPOSProductID)
                                        FROM dba.TranslationPOSProducts
                                        WHERE TranslationID = '" + translation.TranslationID + @"' AND ProductNum = '"+ product.ProductNum +"'";
                                        int CountTranslation = (int)command.ExecuteScalar();
                                        if(CountTranslation == 0)
                                        {
                                            //Insert
                                            command.CommandText = @"INSERT INTO DBA.TranslationPOSProducts (TranslationID, ProductNum, TranslationType, TranslationText)
                                                                    VALUES (?,?,?,?)";
                                            command.Parameters.Clear();
                                            command.Parameters.AddWithValue("TranslationID", Convert.ToInt32(translation.TranslationID));
                                            command.Parameters.AddWithValue("ProductNum", Convert.ToInt32(product.ProductNum));
                                            command.Parameters.AddWithValue("TranslationType", Convert.ToInt32(translation.TranslationType));
                                            command.Parameters.AddWithValue("TranslationText", translation.TranslationText.ToString());
                                            command.ExecuteNonQuery();
                                        }
                                        else
                                        {
                                            //Update
                                            command.CommandText = @"UPDATE DBA.TranslationPOSProducts SET  TranslationType = ?, TranslationText = ?
                                                                    WHERE TranslationID = ? AND ProductNum = ?";
                                            command.Parameters.Clear();
                                            command.Parameters.AddWithValue("TranslationType", Convert.ToInt32(translation.TranslationType));
                                            command.Parameters.AddWithValue("TranslationText", translation.TranslationText.ToString());
                                            command.Parameters.AddWithValue("TranslationID", Convert.ToInt32(translation.TranslationID));
                                            command.Parameters.AddWithValue("ProductNum", Convert.ToInt32(product.ProductNum));
                                            command.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                var msg = new MessageModel { Status = 205, Function = "POSProduct", Message = ex.Message, Description = ex.ToString() };
                                Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            }
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

        [HttpGet]
        public APIResult Get(Nullable<int> ProductNum = null
                            , Nullable<int> ProductID = null
                            , Nullable<int> ReportNo = null
                            , Nullable<int> ProductType = null
                            , Nullable<bool> IsPublic = null
                            , Nullable<bool> IsActive = null
                            , Nullable<int> PageSize = null
                            , Nullable<int> PageNum = null
                            , string OrderBy = null)
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
                try
                {
                    conPixelSqlbase.Open();
                    command.Connection = conPixelSqlbase;
                }
                catch
                {
                    result.Status = 207;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }
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
                string queryin = @"SELECT TOP " + _PageSize + @" START AT " + (_PageNum == 0 ? 1 : ((_PageNum * _PageSize) + 1)) + @" 
                                    p.ProductNum AS 'ProductNum'
                                    FROM DBA.POSProducts p
                                    WHERE ProductNum <> 0";

                string query = @"SELECT TOP " + _PageSize + @" START AT " + (_PageNum == 0 ? 1 : ((_PageNum * _PageSize) + 1)) + @" 
                                    p.ProductID AS 'ProductID',
                                    p.ProductNum AS 'ProductNum',
                                    p.ProductName AS 'ProductName',
                                    p.ReportNo AS 'ReportNo',
                                    p.PriceA AS 'PriceA',
                                    p.PriceB AS 'PriceB',
                                    p.PriceC AS 'PriceC',
                                    p.PriceD AS 'PriceD',
                                    p.PriceE AS 'PriceE',
                                    p.PriceF AS 'PriceF',
                                    p.PriceG AS 'PriceG',
                                    p.PriceH AS 'PriceH',
                                    p.PriceI AS 'PriceI',
                                    p.PriceJ AS 'PriceJ',
                                    p.Tax1 AS 'Tax1',
                                    p.Tax2 AS 'Tax2',
                                    p.Tax3 AS 'Tax3',
                                    p.Tax4 AS 'Tax4',
                                    p.Tax5 AS 'Tax5',
                                    p.Question1 AS 'Question1',
                                    p.Question2 AS 'Question2',
                                    p.Question3 AS 'Question3',
                                    p.Question4 AS 'Question4',
                                    p.Question5 AS 'Question5',
                                    p.ProductType AS 'ProductType',
                                    p.SizeUp AS 'SizeUp',
                                    p.SizeDown AS 'SizeDown',
                                    p.LabelCapacity AS 'LabelCapacity',
                                    p.IsPublic AS 'IsPublic',
                                    ""Index"" AS 'Index',
                                    p.Image AS 'Image',
                                    p.DateCreated AS 'DateCreated',
                                    p.DateModified AS 'DateModified',
                                    p.IsActive AS 'IsActive',
                                    p.TimeStartOrder AS 'TimeStartOrder',
                                    p.TimeEndOrder AS 'TimeEndOrder'
                                    FROM DBA.POSProducts p
                                    WHERE ProductNum <> 0";

                if (ProductNum != null)
                {
                    query += " AND p.ProductNum = " + Convert.ToInt32(ProductNum) + "";
                    queryin += " AND p.ProductNum = " + Convert.ToInt32(ProductNum) + "";
                }
                if (ProductID != null)
                {
                    query += " AND p.ProductID = '" + ProductID + "'";
                    queryin += " AND p.ProductID = '" + ProductID + "'";
                }
                if (ReportNo != null)
                {
                    query += " AND p.ReportNo = '" + ReportNo + "'";
                    queryin += " AND p.ReportNo = '" + ReportNo + "'";
                }
                if (ProductType != null)
                {
                    query += " AND p.ProductType = '" + ProductType + "'";
                    queryin += " AND p.ProductType = '" + ProductType + "'";
                }
                if (IsPublic != null)
                {
                    query += " AND p.IsPublic = '" + (IsPublic == true ? 1 : 0) + "'";
                    queryin += " AND p.IsPublic = '" + (IsPublic == true ? 1 : 0) + "'";
                }
                if (IsActive != null)
                {
                    query += " AND p.IsActive = " + (IsActive == true ? 1 : 0) + "";
                    queryin += " AND p.IsActive = " + (IsActive == true ? 1 : 0) + "";
                }
                string _OrderBy = "ASC";
                if (OrderBy == "DESC")
                {
                    _OrderBy = "DESC";
                }
                query += " ORDER BY p.ProductNum " + _OrderBy + "";
                command.CommandText = query;
                DataTable Data = new DataTable("Products");
                Data.Load(command.ExecuteReader());
                List<POSProductModel> Products = JsonConvert.DeserializeObject<List<POSProductModel>>(JsonConvert.SerializeObject(Data));

                string queryTranlation = "SELECT * FROM DBA.TranslationPOSProducts WHERE ProductNum IN (" + queryin + ")";
                command.CommandText = queryTranlation;
                DataTable DataTranlations = new DataTable("Tranlations");
                DataTranlations.Load(command.ExecuteReader());
                List<TranslationPOSProductModel> Tranlations = JsonConvert.DeserializeObject<List<TranslationPOSProductModel>>(JsonConvert.SerializeObject(DataTranlations));

                var JoinData = (from data in Products
                                select new
                                {
                                    ProductID = data.ProductID,
                                    ProductNum = data.ProductNum,
                                    ProductName = data.ProductName,
                                    ReportNo = data.ReportNo,
                                    PriceA = data.PriceA,
                                    PriceB = data.PriceB,
                                    PriceC = data.PriceC,
                                    PriceD = data.PriceD,
                                    PriceE = data.PriceE,
                                    PriceF = data.PriceF,
                                    PriceG = data.PriceG,
                                    PriceH = data.PriceH,
                                    PriceI = data.PriceI,
                                    PriceJ = data.PriceJ,
                                    Tax1 = data.Tax1,
                                    Tax2 = data.Tax2,
                                    Tax3 = data.Tax3,
                                    Tax4 = data.Tax4,
                                    Tax5 = data.Tax5,
                                    Question1 = data.Question1,
                                    Question2 = data.Question2,
                                    Question3 = data.Question3,
                                    Question4 = data.Question4,
                                    Question5 = data.Question5,
                                    ProductType = data.ProductType,
                                    SizeUp = data.SizeUp,
                                    SizeDown = data.SizeDown,
                                    LabelCapacity = data.LabelCapacity,
                                    IsPublic = data.IsPublic,
                                    Index = data.Index,
                                    Image = data.Image,
                                    DateCreated = data.DateCreated,
                                    DateModified = data.DateModified,
                                    IsActive = data.IsActive,
                                    TimeStartOrder = data.TimeStartOrder,
                                    TimeEndOrder = data.TimeEndOrder,
                                    Tranlations = Tranlations.Where(x=>x.ProductNum  == data.ProductNum).ToList(),
                                }).ToList();

                result.TotalPages = 1;
                result.Status = 200;
                result.Message = "OK";
                result.Data = JoinData;
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
