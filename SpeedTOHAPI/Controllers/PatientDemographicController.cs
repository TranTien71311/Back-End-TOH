using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpeedTOHAPI.Codes;
using SpeedTOHAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace SpeedTOHAPI.Controllers
{
    public class PatientDemographicController : ApiController
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

                //if (!Request.Headers.Contains("Token"))
                //{
                //    result.Status = 203;
                //    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                //    result.Message = msg != null ? msg.Message : "";
                //    return result;
                //}
                //string Token = Request.Headers.GetValues("Token").First();

                //if (!Globals.HMACSHA256(Request.RequestUri.PathAndQuery + "." + body, System.Configuration.ConfigurationManager.AppSettings["SecretKey"].ToString()).Equals(Token))
                //{
                //    result.Status = 202;
                //    var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                //    result.Message = msg != null ? msg.Message : "";
                //    return result;
                //}

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
                int rowIndex = 1;
                List<ErorrModel> Erorrs = new List<ErorrModel>();

                conPixelSqlbase.Open();
                OdbcCommand command = new OdbcCommand();
                command.Connection = conPixelSqlbase;

                OdbcTransaction odbcTransact = null;
                try
                {
                    odbcTransact = conPixelSqlbase.BeginTransaction();
                    command.Transaction = odbcTransact;

                    foreach (var patient in Patients)
                    {
                        if (patient.VisitCode == null)
                        {
                            result.Status = 302;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Erorrs.Add(new ErorrModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        if (patient.HN == null)
                        {
                            result.Status = 303;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Erorrs.Add(new ErorrModel { row = rowIndex, Message = msg });
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
                            Erorrs.Add(new ErorrModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        if (patient.BedCode == null)
                        {
                            result.Status = 305;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Erorrs.Add(new ErorrModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        if (patient.Ward == null)
                        {
                            result.Status = 306;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Erorrs.Add(new ErorrModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        if (patient.PatientFullName == null)
                        {
                            result.Status = 307;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Erorrs.Add(new ErorrModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        if (patient.DoB == null)
                        {
                            result.Status = 308;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Erorrs.Add(new ErorrModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        if (patient.Nationality == null)
                        {
                            result.Status = 309;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Erorrs.Add(new ErorrModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        if (patient.PrimaryDoctor == null)
                        {
                            result.Status = 310;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Erorrs.Add(new ErorrModel { row = rowIndex, Message = msg });
                            continue;
                        }
                        if (patient.FastingFrom == null)
                        {
                            result.Status = 311;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            Erorrs.Add(new ErorrModel { row = rowIndex, Message = msg });
                            continue;
                        }

                      
                        command.CommandText = @"INSERT INTO dba.Patients(VisitCode, HN, BedCode, Ward, PatientFullName, DoB, Nationality, PrimaryDoctor, FastingFrom, FastingTo, LengthOfStay, PreviousBed, MovedToBed, DoNotOrderFrom, DoNotOrderTo)
                                                VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)";
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("VisitCode", patient.VisitCode.ToString());
                        command.Parameters.AddWithValue("HN", patient.HN.ToString());
                        command.Parameters.AddWithValue("BedCode", patient.BedCode.ToString());
                        command.Parameters.AddWithValue("Ward", patient.Ward.ToString());
                        command.Parameters.AddWithValue("PatientFullName", patient.PatientFullName.ToString());
                        command.Parameters.AddWithValue("DoB", Convert.ToDateTime(patient.DoB));
                        command.Parameters.AddWithValue("Nationality", patient.Nationality.ToString());
                        command.Parameters.AddWithValue("PrimaryDoctor", patient.PrimaryDoctor.ToString());
                        command.Parameters.AddWithValue("FastingFrom", Convert.ToDateTime(patient.FastingFrom));
                        command.Parameters.AddWithValue("FastingTo", (patient.FastingTo == null ? null : Convert.ToDateTime(patient.FastingTo).ToString()));
                        command.Parameters.AddWithValue("LengthOfStay", Convert.ToInt32(patient.LengthOfStay));
                        command.Parameters.AddWithValue("PreviousBed", patient.PreviousBed);
                        command.Parameters.AddWithValue("MovedToBed", (patient.MovedToBed == null ? null : Convert.ToDateTime(patient.MovedToBed).ToString()));
                        command.Parameters.AddWithValue("DoNotOrderFrom", (patient.DoNotOrderFrom == null ? null : Convert.ToDateTime(patient.DoNotOrderFrom).ToString()));
                        command.Parameters.AddWithValue("DoNotOrderTo", (patient.DoNotOrderTo == null ? null : Convert.ToDateTime(patient.DoNotOrderTo).ToString()));
                        command.ExecuteNonQuery();

                        rowIndex++;
                    }
                    if(Erorrs.Count > 0)
                    {
                        odbcTransact.Rollback();
                        result.Erorr = Erorrs;
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
    }
}