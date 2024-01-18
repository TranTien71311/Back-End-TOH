using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpeedTOHAPI.Codes;
using SpeedTOHAPI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Results;

namespace SpeedTOHAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class PatientDemographicsController : ApiController
    {
        [HttpPost]
        public APIResult POSTPatientDemographic([NakedBody] string body)
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

                List<PatientModel> Patients = JsonConvert.DeserializeObject<List<PatientModel>>(body);

                if (Patients.Count() < 0)
                {
                    result.Status = 301;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }
                if (Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["MaxLengthAPI"]) < Patients.Count())
                {
                    result.Status = 208;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }
                int rowIndex = 0;
                List<ErrorModel> Errors = new List<ErrorModel>();

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

                OdbcTransaction odbcTransact = null;
                try
                {
                    odbcTransact = conPixelSqlbase.BeginTransaction();
                    command.Transaction = odbcTransact;

                    foreach (var patient in Patients)
                    {
                        rowIndex++;
                        if (patient.VisitCode == null)
                        {
                            result.Status = 302;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        if (patient.HN == null)
                        {
                            result.Status = 303;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        command.CommandText = @"SELECT COUNT(PatientID)
                                        FROM dba.Patients
                                        WHERE VisitCode = '" + patient.VisitCode + @"'
                                          AND HN = '" + patient.HN + @"'
                                          AND IsActive = 1";
                        int CountPatient = (int)command.ExecuteScalar();

                        if (CountPatient > 0)
                        {
                            result.Status = 304;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        if (patient.BedCode == null)
                        {
                            result.Status = 305;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        if (patient.Ward == null)
                        {
                            result.Status = 306;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        if (patient.PatientFullName == null)
                        {
                            result.Status = 307;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        if (patient.DoB == null)
                        {
                            result.Status = 308;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        if (patient.Nationality == null)
                        {
                            result.Status = 309;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        if (patient.PrimaryDoctor == null)
                        {
                            result.Status = 310;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        if (patient.FastingFrom == null)
                        {
                            result.Status = 311;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }

                      
                        command.CommandText = @"INSERT INTO dba.Patients(VisitCode, HN, BedCode, Ward, PatientFullName, DoB, Nationality, PrimaryDoctor, FastingFrom, FastingTo, LengthOfStay, PreviousBed, MovedToBed, DoNotOrderFrom, DoNotOrderTo, DischargeDate, AdmitDate)
                                                VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)";
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("VisitCode", patient.VisitCode.ToString());
                        command.Parameters.AddWithValue("HN", patient.HN.ToString());
                        command.Parameters.AddWithValue("BedCode", patient.BedCode.ToString());
                        command.Parameters.AddWithValue("Ward", patient.Ward.ToString());
                        command.Parameters.AddWithValue("PatientFullName", patient.PatientFullName.ToString());
                        command.Parameters.AddWithValue("DoB", Convert.ToDateTime(patient.DoB).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture));
                        command.Parameters.AddWithValue("Nationality", patient.Nationality.ToString());
                        command.Parameters.AddWithValue("PrimaryDoctor", patient.PrimaryDoctor.ToString());
                        command.Parameters.AddWithValue("FastingFrom", Convert.ToDateTime(patient.FastingFrom).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture));
                        command.Parameters.AddWithValue("FastingTo", (patient.FastingTo == null ? null : Convert.ToDateTime(patient.FastingTo).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture)));
                        command.Parameters.AddWithValue("LengthOfStay", Convert.ToInt32(patient.LengthOfStay));
                        command.Parameters.AddWithValue("PreviousBed", patient.PreviousBed);
                        command.Parameters.AddWithValue("MovedToBed", (patient.MovedToBed == null ? null : patient.MovedToBed));
                        command.Parameters.AddWithValue("DoNotOrderFrom", (patient.DoNotOrderFrom == null ? null : Convert.ToDateTime(patient.DoNotOrderFrom).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture)));
                        command.Parameters.AddWithValue("DoNotOrderTo", (patient.DoNotOrderTo == null ? null : Convert.ToDateTime(patient.DoNotOrderTo).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture)));
                        command.Parameters.AddWithValue("DischargeDate", (patient.DischargeDate == null ? null : Convert.ToDateTime(patient.DischargeDate).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture)));
                        command.Parameters.AddWithValue("AdmitDate", (patient.DoNotOrderTo == null ? null : Convert.ToDateTime(patient.AdmitDate).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture)));
                        command.ExecuteNonQuery();
                    }
                    if(Errors.Count > 0)
                    {
                        odbcTransact.Rollback();
                        result.Error = Errors;
                    }
                    else
                    {
                        result.Status = 200;
                        var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                        result.Message = msg != null ? msg.Message : "";
                        odbcTransact.Commit();
                    }
                }
                catch(Exception ex)
                {
                    if (odbcTransact != null)
                        odbcTransact.Rollback();
                    result.Status = 0;
                    result.Message = ex.Message;
                    result.Exception = ex.ToString();
                }
                
            }
            catch(Exception ex)
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
        public APIResult PUTPatientDemographic([NakedBody] string body)
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

                List<PatientModel> Patients = JsonConvert.DeserializeObject<List<PatientModel>>(body);

                if (Patients.Count() < 0)
                {
                    result.Status = 301;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }
                if (Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["MaxLengthAPI"]) < Patients.Count())
                {
                    result.Status = 208;
                    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                    result.Message = msg != null ? msg.Message : "";
                    return result;
                }
                int rowIndex = 0;
                List<ErrorModel> Errors = new List<ErrorModel>();
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

                OdbcTransaction odbcTransact = null;
                try
                {
                    odbcTransact = conPixelSqlbase.BeginTransaction();
                    command.Transaction = odbcTransact;

                    foreach (var patient in Patients)
                    {
                        rowIndex++;

                        if (patient.VisitCode == null)
                        {
                            result.Status = 302;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        if (patient.HN == null)
                        {
                            result.Status = 303;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        command.CommandText = @"SELECT COUNT(PatientID)
                                        FROM dba.Patients
                                        WHERE VisitCode = '" + patient.VisitCode + @"'
                                          AND HN = '" + patient.HN + @"'
                                          AND IsActive = 1";
                        int CountPatient = (int)command.ExecuteScalar();

                        if (CountPatient == 0)
                        {
                            result.Status = 312;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Errors.Add(new ErrorModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        string query = "UPDATE DBA.Patients SET ModifiedDate= ? ";
                        if (patient.BedCode != null)
                        {
                            query += ", BedCode='" + patient.BedCode + "'";
                        }
                        if (patient.Ward != null)
                        {
                            query += ", Ward='" + patient.Ward + "'";
                        }
                        if (patient.PatientFullName != null)
                        {
                            query += ", PatientFullName='" + patient.PatientFullName + "'";
                        }
                        if (patient.DoB != null)
                        {
                            query += ", DoB='" + Convert.ToDateTime(patient.DoB) + "'";
                        }
                        if (patient.Nationality != null)
                        {
                            query += ", Nationality='" + patient.Nationality + "'";
                        }
                        if (patient.PrimaryDoctor != null)
                        {
                            query += ", PrimaryDoctor='" + patient.PrimaryDoctor + "'";
                        }
                        if (patient.FastingFrom != null)
                        {
                            query += ", FastingFrom='" + Convert.ToDateTime(patient.FastingFrom).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) + "'";
                        }
                        if (patient.FastingTo != null)
                        {
                            query += ", FastingTo='" + Convert.ToDateTime(patient.FastingTo).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) + "'";
                        }
                        if (patient.LengthOfStay != null)
                        {
                            query += ", LengthOfStay=" + patient.LengthOfStay + "";
                        }
                        if (patient.PreviousBed != null)
                        {
                            query += ", PreviousBed='" + patient.PreviousBed + "'";
                        }
                        if (patient.MovedToBed != null)
                        {
                            query += ", MovedToBed='" + patient.MovedToBed + "'";
                        }
                        if (patient.DoNotOrderFrom != null)
                        {
                            query += ", DoNotOrderFrom='" + Convert.ToDateTime(patient.DoNotOrderFrom).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) + "'";
                        }
                        if (patient.DoNotOrderTo != null)
                        {
                            query += ", DoNotOrderTo='" + Convert.ToDateTime(patient.DoNotOrderTo).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) + "'";
                        }
                        if (patient.AdmitDate != null)
                        {
                            query += ", AdmitDate='" + Convert.ToDateTime(patient.AdmitDate).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) + "'";
                        }
                        if (patient.DischargeDate != null)
                        {
                            query += ", DischargeDate='" + Convert.ToDateTime(patient.DischargeDate).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) + "'";
                        }
                        if (patient.IsActive != null)
                        {
                            query += ", IsActive='" + patient.IsActive + "'";
                        }
                        query += " WHERE VisitCode='"+ patient.VisitCode + "' AND HN='"+ patient.HN + "'";

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
        public APIResult GETPatientDemographic(Nullable<int> PantientID = null,
                                               string VisitCode = null,
                                               string HN = null,
                                               string BedCode = null,
                                               string DoB = null,
                                               string FastingFrom = null,
                                               string FastingTo = null,
                                               Nullable<int> LengthOfStay = null,
                                               string PreviousBed = null,
                                               string MovedToBed = null,
                                               string DoNotOrderFrom = null,
                                               string DoNotOrderTo = null,
                                               Nullable<bool> IsActive = null,
                                               string CreatedFrom = null,
                                               string CreatedTo = null,
                                               string ModifiedFrom = null,
                                               string ModifiedTo = null,
                                               string AdmitDate = null,
                                               string DischargeDate = null,
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
                //try
                //{
                    conPixelSqlbase.Open();
                    command.Connection = conPixelSqlbase;
                //}
                //catch
                //{
                //    result.Status = 207;
                //    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                //    result.Message = msg != null ? msg.Message : "";
                //    return result;
                //}
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
                                    PA.PatientID,
                                    PA.VisitCode,
                                    PA.HN,
                                    PA.BedCode,
                                    PA.Ward,
                                    PA.PatientFullName,
                                    PA.DoB,
                                    PA.Nationality,
                                    PA.PrimaryDoctor,
                                    PA.FastingFrom,
                                    PA.FastingTo,
                                    PA.LengthOfStay,
                                    PA.PreviousBed,
                                    PA.MovedToBed,
                                    PA.DoNotOrderFrom,
                                    PA.DoNotOrderTo,
                                    PA.AdmitDate,
                                    PA.DischargeDate,
                                    PA.IsActive,
                                    PA.CreatedDate,
                                    PA.ModifiedDate
                                    FROM DBA.Patients PA
                                    WHERE PatientID <> 0";

                if (PantientID != null)
                {
                    query += " AND PantientID = " + Convert.ToInt32(PantientID) + "";
                }
                if (VisitCode != null)
                {
                    query += " AND VisitCode = '" + VisitCode + "'";
                }
                if (HN != null)
                {
                    query += " AND HN = '" + HN + "'";
                }
                if (BedCode != null)
                {
                    query += " AND BedCode = '" + BedCode + "'";
                }
                if (DoB != null)
                {
                    query += " AND DoB = '" + DoB + "'";
                }
                if (FastingFrom != null)
                {
                    DateTime Date = DateTime.Parse(FastingFrom);
                    query += " AND FastingFrom >= '" + Date.ToString("yyyy/MM/dd 00:00:00", CultureInfo.InvariantCulture) + "'";
                }
                if (FastingTo != null)
                {
                    DateTime Date = DateTime.Parse(FastingTo);
                    query += " AND FastingTo <= '" + Date.ToString("yyyy/MM/dd 23:59:59", CultureInfo.InvariantCulture) + "'";
                }
                if (LengthOfStay != null)
                {
                    query += " AND LengthOfStay = " + LengthOfStay + "";
                }
                if (PreviousBed != null)
                {
                    query += " AND PreviousBed = '" + PreviousBed + "'";
                }
                if (MovedToBed != null)
                {
                    query += " AND MovedToBed = '" + MovedToBed + "'";
                }
                if (DoNotOrderFrom != null)
                {
                    DateTime Date = DateTime.Parse(DoNotOrderFrom);
                    query += " AND DoNotOrderFrom >= '" + Date.ToString("yyyy/MM/dd 23:59:59", CultureInfo.InvariantCulture) + "'";
                }
                if (DoNotOrderTo != null)
                {
                    DateTime Date = DateTime.Parse(DoNotOrderTo);
                    query += " AND DoNotOrderTo <= '" + Date.ToString("yyyy/MM/dd 23:59:59", CultureInfo.InvariantCulture) + "'";
                }
                if (AdmitDate != null)
                {
                    DateTime Date = DateTime.Parse(AdmitDate);
                    query += " AND AdmitDate = '" + Date.ToString("yyyy/MM/dd 23:59:59", CultureInfo.InvariantCulture) + "'";
                }
                if (DischargeDate != null)
                {
                    DateTime Date = DateTime.Parse(DischargeDate);
                    query += " AND DischargeDate = '" + Date.ToString("yyyy/MM/dd 23:59:59", CultureInfo.InvariantCulture) + "'";
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
                query += " ORDER BY PatientID " + _OrderBy + "";
                command.CommandText = query;
                DataTable Data = new DataTable("Patients");
                Data.Load(command.ExecuteReader());

                result.Status = 200;
                result.Message = "OK";
                result.Data = Data;
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