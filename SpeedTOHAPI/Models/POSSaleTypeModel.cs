using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class POSSaleTypeModel
    {
        public int? POSSaleTypeID { get; set; }
        public int? SaleTypeIndex { get; set; }
        public string Descript { get; set; }
        public double? ForcePrice { get; set; }
        public int? Tax1Exempt { get; set; }
        public int? Tax2Exempt { get; set; }
        public int? Tax3Exempt { get; set; }
        public int? Tax4Exempt { get; set; }
        public int? Tax5Exempt { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? IsActive { get; set; }
    }
}