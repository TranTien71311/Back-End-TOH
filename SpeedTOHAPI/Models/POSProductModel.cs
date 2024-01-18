using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class POSProductModel
    {
        public int? ProductID { get; set; }
        public int? ProductNum { get; set; }
        public string ProductName { get; set; }
        public int? ReportNo { get; set; }
        public double? PriceA { get; set; }
        public double? PriceB { get; set; }
        public double? PriceC { get; set; }
        public double? PriceD { get; set; }
        public double? PriceE { get; set; }
        public double? PriceF { get; set; }
        public double? PriceG { get; set; }
        public double? PriceH { get; set; }
        public double? PriceI { get; set; }
        public double? PriceJ { get; set; }
        public bool? Tax1 { get; set; }
        public bool? Tax2 { get; set; }
        public bool? Tax3 { get; set; }
        public bool? Tax4 { get; set; }
        public bool? Tax5 { get; set; }
        public int? ProductType { get; set; }
        public bool? SizeUp {  get; set; }
        public bool? SizeDown {  get; set; }
        public string LabelCapacity {  get; set; }
        public bool? IsPublic { get; set; }
        public int? Index {  get; set; }
        public string Image { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set;}
        public bool? IsActive { get; set; }
        public List<TranslationPOSProductModel> Translations { get; set; }

    }
}