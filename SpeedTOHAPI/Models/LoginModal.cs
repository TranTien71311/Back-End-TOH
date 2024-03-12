using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class LoginModal
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string EmployeeCode { get; set; }
        public int? TypeLogin {  get; set; }
    }
}