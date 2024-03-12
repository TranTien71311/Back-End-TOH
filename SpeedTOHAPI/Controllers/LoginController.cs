using Newtonsoft.Json;
using SpeedTOHAPI.Codes;
using SpeedTOHAPI.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace SpeedTOHAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class LoginController : ApiController
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

                LoginModal Login = JsonConvert.DeserializeObject<LoginModal>(body);

                OdbcCommand command = new OdbcCommand();
                conPixelSqlbase.Open();
                command.Connection = conPixelSqlbase;

                OdbcTransaction odbcTransact = null;
                try
                {
                    odbcTransact = conPixelSqlbase.BeginTransaction();
                    command.Transaction = odbcTransact;

                    if (Login.TypeLogin == null)
                    {
                        result.Status = 1401;
                        var msg1 = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                        result.Message = msg1 != null ? msg1.Message : "";
                        return result;
                    }
                    else
                    {
                        if(Login.TypeLogin == 0)
                        {
                            if(Login.UserName == null)
                            {
                                result.Status = 1402;
                                var msg1 = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                result.Message = msg1 != null ? msg1.Message : "";
                                return result;
                            }
                            if (Login.Password == null)
                            {
                                result.Status = 1403;
                                var msg1 = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                result.Message = msg1 != null ? msg1.Message : "";
                                return result;
                            }

                            command.CommandText = @"SELECT u.UserID, u.UserName, u.Email, u.EmployeeCode, ug.UserGroupName, ug.UserGroupID, ug.AccessAllPermission, u.FullName
                                                    FROM DBA.Users u 
                                                    LEFT JOIN DBA.UserGroups ug ON u.UserGroupID = ug.UserGroupID
                                                    WHERE UserName='" + Login.UserName +"' AND Password='"+ Login.Password +"' AND u.IsActive=1";
                            DataTable DataUser = new DataTable("User");
                            DataUser.Load(command.ExecuteReader());
                            if(DataUser.Rows.Count == 0)
                            {
                                result.Status = 1407;
                                var msg1 = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                result.Message = msg1 != null ? msg1.Message : "";
                                return result;
                            }
                            List<UserModal> User = JsonConvert.DeserializeObject<List<UserModal>>(JsonConvert.SerializeObject(DataUser));

                            List<string> Permission = new List<string>();
                            if (User[0].AccessAllPermission == true)
                            {
                                command.CommandText = "SELECT PermissionCode FROM DBA.Permissions WHERE IsActive=1";
                                DataTable DataPermission = new DataTable("Permission");
                                DataPermission.Load(command.ExecuteReader());
                                for(int i = 0; i < DataPermission.Rows.Count ; i++)
                                {
                                    Permission.Add(DataPermission.Rows[i]["PermissionCode"].ToString());
                                }
                            }
                            else
                            {
                                command.CommandText = @"SELECT PermissionCode FROM DBA.Permissions p
                                                        LEFT JOIN DBA.Functions f ON p.PermissionID = f.PermissionID
                                                        WHERE f.UserGroupID='" + User[0].UserGroupID + "' AND f.IsActive=1";
                                DataTable DataPermission = new DataTable("Permission");
                                DataPermission.Load(command.ExecuteReader());
                                for (int i = 0; i < DataPermission.Rows.Count; i++)
                                {
                                    Permission.Add(DataPermission.Rows[i]["PermissionCode"].ToString());
                                }
                            }

                            UserModal UserResult = new UserModal();
                            UserResult.FullName = User[0].FullName;
                            UserResult.UserGroupName = User[0].UserGroupName;
                            UserResult.Email = User[0].Email;
                            UserResult.EmployeeCode = User[0].EmployeeCode;
                            UserResult.Permission = Permission;

                            result.Status = 200;
                            result.Data = UserResult;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            result.Message = msg != null ? msg.Message : "";
                        }
                        if (Login.TypeLogin == 1)
                        {
                            if (Login.Email == null)
                            {
                                result.Status = 1404;
                                var msg1 = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                result.Message = msg1 != null ? msg1.Message : "";
                                return result;
                            }
                            if (Login.Password == null)
                            {
                                result.Status = 1403;
                                var msg1 = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                result.Message = msg1 != null ? msg1.Message : "";
                                return result;
                            }

                            command.CommandText = @"SELECT u.UserID, u.UserName, u.Email, u.EmployeeCode, ug.UserGroupName, ug.UserGroupID, ug.AccessAllPermission, u.FullName
                                                    FROM DBA.Users u 
                                                    LEFT JOIN DBA.UserGroups ug ON u.UserGroupID = ug.UserGroupID
                                                    WHERE Email='" + Login.Email + "' AND Password='" + Login.Password + "' AND u.IsActive=1";
                            DataTable DataUser = new DataTable("User");
                            DataUser.Load(command.ExecuteReader());
                            if (DataUser.Rows.Count == 0)
                            {
                                result.Status = 1407;
                                var msg1 = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                result.Message = msg1 != null ? msg1.Message : "";
                                return result;
                            }
                            List<UserModal> User = JsonConvert.DeserializeObject<List<UserModal>>(JsonConvert.SerializeObject(DataUser));

                            List<string> Permission = new List<string>();
                            if (User[0].AccessAllPermission == true)
                            {
                                command.CommandText = "SELECT PermissionCode FROM DBA.Permissions WHERE IsActive=1";
                                DataTable DataPermission = new DataTable("Permission");
                                DataPermission.Load(command.ExecuteReader());
                                for (int i = 0; i < DataPermission.Rows.Count; i++)
                                {
                                    Permission.Add(DataPermission.Rows[i]["PermissionCode"].ToString());
                                }
                            }
                            else
                            {
                                command.CommandText = @"SELECT PermissionCode FROM DBA.Permissions p
                                                        LEFT JOIN DBA.Functions f ON p.PermissionID = f.PermissionID
                                                        WHERE f.UserGroupID='" + User[0].UserGroupID + "' AND f.IsActive=1";
                                DataTable DataPermission = new DataTable("Permission");
                                DataPermission.Load(command.ExecuteReader());
                                for (int i = 0; i < DataPermission.Rows.Count; i++)
                                {
                                    Permission.Add(DataPermission.Rows[i]["PermissionCode"].ToString());
                                }
                            }

                            UserModal UserResult = new UserModal();
                            UserResult.FullName = User[0].FullName;
                            UserResult.UserGroupName = User[0].UserGroupName;
                            UserResult.Email = User[0].Email;
                            UserResult.EmployeeCode = User[0].EmployeeCode;
                            UserResult.Permission = Permission;

                            result.Status = 200;
                            result.Data = UserResult;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            result.Message = msg != null ? msg.Message : "";
                        }
                        if (Login.TypeLogin == 2)
                        {
                            if (Login.EmployeeCode == null)
                            {
                                result.Status = 1405;
                                var msg1 = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                result.Message = msg1 != null ? msg1.Message : "";
                                return result;
                            }

                            command.CommandText = @"SELECT u.UserID, u.UserName, u.Email, u.EmployeeCode, ug.UserGroupName, ug.UserGroupID, ug.AccessAllPermission, u.FullName
                                                    FROM DBA.Users u 
                                                    LEFT JOIN DBA.UserGroups ug ON u.UserGroupID = ug.UserGroupID
                                                    WHERE EmployeeCode='" + Login.EmployeeCode + "' AND u.IsActive=1";
                            DataTable DataUser = new DataTable("User");
                            DataUser.Load(command.ExecuteReader());
                            if (DataUser.Rows.Count == 0)
                            {
                                result.Status = 1407;
                                var msg1 = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                                result.Message = msg1 != null ? msg1.Message : "";
                                return result;
                            }
                            List<UserModal> User = JsonConvert.DeserializeObject<List<UserModal>>(JsonConvert.SerializeObject(DataUser));

                            List<string> Permission = new List<string>();
                            if (User[0].AccessAllPermission == true)
                            {
                                command.CommandText = "SELECT PermissionCode FROM DBA.Permissions WHERE IsActive=1";
                                DataTable DataPermission = new DataTable("Permission");
                                DataPermission.Load(command.ExecuteReader());
                                for (int i = 0; i < DataPermission.Rows.Count; i++)
                                {
                                    Permission.Add(DataPermission.Rows[i]["PermissionCode"].ToString());
                                }
                            }
                            else
                            {
                                command.CommandText = @"SELECT PermissionCode FROM DBA.Permissions p
                                                        LEFT JOIN DBA.Functions f ON p.PermissionID = f.PermissionID
                                                        WHERE f.UserGroupID='" + User[0].UserGroupID + "' AND f.IsActive=1";
                                DataTable DataPermission = new DataTable("Permission");
                                DataPermission.Load(command.ExecuteReader());
                                for (int i = 0; i < DataPermission.Rows.Count; i++)
                                {
                                    Permission.Add(DataPermission.Rows[i]["PermissionCode"].ToString());
                                }
                            }

                            UserModal UserResult = new UserModal();
                            UserResult.FullName = User[0].FullName;
                            UserResult.UserGroupName = User[0].UserGroupName;
                            UserResult.Email = User[0].Email;
                            UserResult.EmployeeCode = User[0].EmployeeCode;
                            UserResult.Permission = Permission;

                            result.Status = 200;
                            result.Data = UserResult;
                            var msg = Globals.GetStatusCode().Where(x => x.Status == result.Status).SingleOrDefault();
                            result.Message = msg != null ? msg.Message : "";
                        }
                    } 
                }
                catch (Exception ex)
                {
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
