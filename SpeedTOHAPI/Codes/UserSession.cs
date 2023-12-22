using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Codes
{
    [Serializable]
    public class UserSession
    {
        public int UserId { set; get; }
        public string UserName { set; get; }
    }
}