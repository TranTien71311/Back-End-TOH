using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class ErrorModel
    {
        public int row { get; set; }
        public MessageModel Message { get; set; }
    }
}