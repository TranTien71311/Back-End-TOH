using Newtonsoft.Json;
using SpeedTOHAPI.Codes;
using SpeedTOHAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace SpeedTOHAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class POSProductCombosController : ApiController
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

                List<POSProductComboModel> ProductCombos = JsonConvert.DeserializeObject<List<POSProductComboModel>>(body);
                if (ProductCombos.Count() < 0)
                {
                    result.Status = 603;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }
                if (Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["MaxLengthAPI"]) < ProductCombos.Count())
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

                    foreach (var productcombo in ProductCombos)
                    {
                        rowIndex++;
                        if (productcombo.ProductComboID == null)
                        {
                            result.Status = 602;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        else
                        {
                            command.CommandText = @"SELECT ISNULL(ProductNum, -1)
                                        FROM dba.POSProductCombos
                                        WHERE ProductComboID = '" + productcombo.ProductComboID + @"'";
                            int ProductComboID = (int)command.ExecuteScalar();
                            if (ProductComboID == -1)
                            {
                                result.Status = 611;
                                var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                continue;
                            }
                            //UPDATE
                            try
                            {
                                string query = "UPDATE DBA.POSProductCombos SET ModifiedDate= ? ";
                                if (productcombo.IsPublic != null)
                                {
                                    query += ", IsPublic=" + productcombo.IsPublic + "";
                                }
                                query += " WHERE ProductComboID='" + productcombo.ProductComboID + "'";

                                command.CommandText = query;
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("ModifiedDate", Convert.ToDateTime((DateTime.Now).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture)));
                                command.ExecuteNonQuery();

                                if (productcombo.Translations.Count() > 0)
                                {
                                    foreach (var translation in productcombo.Translations)
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
                                            result.Status = 615;
                                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                                            continue;
                                        }
                                        command.CommandText = @"SELECT ISNULL(TranslationPOSProductID, -1)
                                        FROM dba.TranslationPOSProductCombos
                                        WHERE TranslationID = '" + translation.TranslationID + @"' AND ProductComboID = '" + productcombo.ProductComboID + "'";
                                        int TranslationPOSProductComboID = (int)command.ExecuteScalar();
                                        if (TranslationPOSProductComboID == -1)
                                        {
                                            //Insert
                                            command.CommandText = @"INSERT INTO DBA.TranslationPOSProductCombos (TranslationID, ProductComboID, TranslationType, TranslationText)
                                                                    VALUES (?,?,?,?)";
                                            command.Parameters.Clear();
                                            command.Parameters.AddWithValue("TranslationID", Convert.ToInt32(translation.TranslationID));
                                            command.Parameters.AddWithValue("ProductComboID", Convert.ToInt32(translation.ProductComboID));
                                            command.Parameters.AddWithValue("TranslationType", Convert.ToInt32(translation.TranslationType));
                                            command.Parameters.AddWithValue("TranslationText", translation.TranslationText.ToString());
                                            command.ExecuteNonQuery();
                                        }
                                        else
                                        {
                                            //Update
                                            command.CommandText = @"UPDATE DBA.TranslationPOSProductCombos SET TranslationID = ?, ProductComboID = ?, TranslationType = ?, TranslationText = ?
                                                                    WHERE TranslationPOSProductComboID = ?";
                                            command.Parameters.Clear();
                                            command.Parameters.AddWithValue("TranslationID", Convert.ToInt32(translation.TranslationID));
                                            command.Parameters.AddWithValue("ProductComboID", Convert.ToInt32(translation.ProductComboID));
                                            command.Parameters.AddWithValue("TranslationType", Convert.ToInt32(translation.TranslationType));
                                            command.Parameters.AddWithValue("TranslationText", translation.TranslationText.ToString());
                                            command.Parameters.AddWithValue("TranslationPOSProductComboID", Convert.ToInt32(TranslationPOSProductComboID));
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
                        result.Data = ProductCombos;
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
        public APIResult Get(Nullable<int> ProductComboID = null
                            , Nullable<int> POSProductComboID = null
                            , Nullable<int> ProdLinkNum = null
                            , Nullable<int> Sequence = null
                            , Nullable<int> ProductNum = null
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
                                    p.ProductComboID AS 'ProductComboID'
                                    FROM DBA.POSProductCombos p
                                    WHERE ProductComboID <> 0";

                string query = @"SELECT TOP " + _PageSize + @" START AT " + (_PageNum == 0 ? 1 : ((_PageNum * _PageSize) + 1)) + @" 
                                    p.POSProductComboID AS 'POSProductComboID',
                                    p.ProductComboID AS 'ProductComboID',
                                    p.ProdLinkNum AS 'ProdLinkNum',
                                    p.ProdLinkName AS 'ProdLinkName',
                                    p.Sequence AS 'Sequence',
                                    p.ProductNum AS 'ProductNum',
                                    p.ProductName AS 'ProductName',
                                    p.Price AS 'Price',
                                    p.IsPublic AS 'IsPublic',
                                    p.DateCreated AS 'DateCreated',
                                    p.DateModified AS 'DateModified',
                                    p.IsActive AS 'IsActive',
                                    FROM DBA.POSProductCombos p
                                    WHERE ProductComboID <> 0";

                if (POSProductComboID != null)
                {
                    query += " AND p.POSProductComboID = " + Convert.ToInt32(POSProductComboID) + "";
                    queryin += " AND p.POSProductComboID = " + Convert.ToInt32(POSProductComboID) + "";
                }
                if (ProductComboID != null)
                {
                    query += " AND p.ProductComboID = '" + ProductComboID + "'";
                    queryin += " AND p.ProductComboID = '" + ProductComboID + "'";
                }
                if (ProdLinkNum != null)
                {
                    query += " AND p.ProdLinkNum = '" + ProdLinkNum + "'";
                    queryin += " AND p.ProdLinkNum = '" + ProdLinkNum + "'";
                }
                if (Sequence != null)
                {
                    query += " AND p.Sequence = '" + Sequence + "'";
                    queryin += " AND p.Sequence = '" + Sequence + "'";
                }
                if (Sequence != null)
                {
                    query += " AND p.ProductNum = '" + ProductNum + "'";
                    queryin += " AND p.ProductNum = '" + ProductNum + "'";
                }
                if (IsPublic != null)
                {
                    query += " AND p.IsPublic = '" + (IsPublic == true ? 1 : 0) + "'";
                    queryin += " AND p.IsPublic = '" + (IsPublic == true ? 1 : 0) + "'";
                }
                if (IsActive != null)
                {
                    query += " AND d.IsActive = " + (IsActive == true ? 1 : 0) + "";
                    queryin += " AND d.IsActive = " + (IsActive == true ? 1 : 0) + "";
                }
                string _OrderBy = "ASC";
                if (OrderBy == "DESC")
                {
                    _OrderBy = "DESC";
                }
                query += " ORDER BY p.ProductComboID " + _OrderBy + "";
                command.CommandText = query;
                DataTable Data = new DataTable("ProductCombos");
                Data.Load(command.ExecuteReader());
                List<POSProductComboModel> ProductCombos = JsonConvert.DeserializeObject<List<POSProductComboModel>>(JsonConvert.SerializeObject(Data));

                string queryTranlation = "SELECT * FROM DBA.TranlationPOSProductCombos WHERE ProductComboID IN (" + queryin + ")";
                command.CommandText = queryTranlation;
                DataTable DataTranlations = new DataTable("Tranlations");
                DataTranlations.Load(command.ExecuteReader());
                List<TranslationPOSProductModel> Tranlations = JsonConvert.DeserializeObject<List<TranslationPOSProductModel>>(JsonConvert.SerializeObject(DataTranlations));

                var JoinData = (from data in ProductCombos
                                select new
                                {
                                    POSProductComboID = data.POSProductComboID,
                                    ProductComboID = data.ProductComboID,
                                    ProdLinkNum = data.ProdLinkNum,
                                    ProdLinkName = data.ProdLinkName,
                                    Sequence = data.Sequence,
                                    ProductNum = data.ProductNum,
                                    ProductName = data.ProductName,
                                    Price = data.Price,
                                    IsPublic = data.IsPublic,
                                    DateCreated = data.DateCreated,
                                    DateModified = data.DateModified,
                                    IsActive = data.IsActive,
                                    Tranlations = Tranlations.Where(x => x.ProductNum == data.ProductNum).ToList(),
                                }).ToList();

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
    }
}
