using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class TranslationSubCategoryModel
    {
        public int? TranslationSubCategoryID { get; set; }
        public int? TranslationID { get; set; }
        public int? SubCategoryCode { get; set; }
        public int? TranslationType { get; set; }
        public string TranslationText { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
    }
}