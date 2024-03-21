using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class POSConfigModel
    {
        public int? SaleType { get; set; }
        public string ProductComment { get; set; }
        public int? StationNum { get; set; }
        public int? RevCenter { get; set; }
        public int? SectionNum {  get; set; }
        public int? TableNum { get; set; }
        public int? MemberCode { get; set; }
    }
}