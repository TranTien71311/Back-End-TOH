using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Codes
{
    public class APIResult
    {
        public int Status { set; get; }
        public string Message { set; get; }
        public string Exception { set; get; }
        public object Data { set; get; }
    }
}