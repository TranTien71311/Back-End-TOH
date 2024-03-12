using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class UserModal
    {
        public int? UserID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string EmployeeCode { get; set; }
        public List<string> Permission { get; set; }
        public string UserGroupName { get; set; }
        public int? UserGroupID { get; set; }
        public bool? AccessAllPermission { get; set; }
        public string FullName { get; set; }
    }
}