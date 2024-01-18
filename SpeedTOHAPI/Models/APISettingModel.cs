using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class APISettingModel
    {
        public string POSAPILink { get; set; }
        public string SecretKey { get; set; }
        public string PartnerKey {  get; set; }
    }
}