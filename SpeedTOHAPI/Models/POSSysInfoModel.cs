using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class POSSysInfoModel
    {
        public int? POSSysInfoID {  get; set; }
        public string Company { get; set; }
        public string TaxDes1 { get; set; }
        public string TaxDes2 { get; set; }
        public string TaxDes3 { get; set; }
        public string TaxDes4 { get; set; }
        public string TaxDes5 { get; set; }
        public int? TaxRate1 { get; set; }
        public int? TaxRate2 { get; set; }
        public int? TaxRate3 { get; set; }
        public int? TaxRate4 { get; set; }
        public int? TaxRate5 { get; set; }
        public int? TaxData1 { get; set; }
        public int? TaxData2 { get; set; }
        public int? TaxData3 { get; set; }
        public int? TaxData4 { get; set; }
        public int? TaxData5 { get; set; }
        public int? UseVAT { get; set; }
    }
}