using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class MessageModel
    {
        public int Status { set; get; }
        public string Message { set; get; }
        public string Description { set; get; }
        public string Function { set; get; }
    }
}