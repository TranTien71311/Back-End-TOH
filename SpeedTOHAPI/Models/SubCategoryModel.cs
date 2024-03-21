using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class SubCategoryModel
    {
        public int? SubCategoryID { get; set; }
        public int? SubCategoryCode { get; set; }
        public int? CategoryCode { get; set; }
        public string SubCategoryName { get; set; }
        public string Image { get; set; }
        public int? Index { get; set; }
        public bool? IsPublic { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        public List<TranslationPOSReportCatModel> Translations { get; set; }
    }
}