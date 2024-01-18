using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class WardModel
    {
        public int? Index { get; set; }
        public string WardID { get; set; }
        public string WardNameEn { get; set; }
        public string WardNameVn { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}