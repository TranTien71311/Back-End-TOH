using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class ProductModel
    {
        public int? ProductID { get; set; }
        public int? ProductCode { get; set; }
        public string ProductName { get; set; }
        public int? SubCategoryCode { get; set; }
        public double? Price { get; set; }
        public bool? Tax1 { get; set; }
        public bool? Tax2 { get; set; }
        public bool? Tax3 { get; set; }
        public bool? Tax4 { get; set; }
        public bool? Tax5 { get; set; }
        public int? ProductType { get; set; }
        public bool? IsPublic { get; set; }
        public int? Index { get; set; }
        public string Image { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        public bool? IsActive { get; set; }
        public List<TranslationPOSProductModel> Translations { get; set; }
        public int? Question1 { get; set; }
        public int? Question2 { get; set; }
        public int? Question3 { get; set; }
        public int? Question4 { get; set; }
        public int? Question5 { get; set; }
        public string TimeStartOrder { get; set; }
        public string TimeEndOrder { get; set; }
        public string Kcal { get; set; }
    }
}