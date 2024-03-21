using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class CategoryModel
    {
        public int? CategoryID { get; set; }
        public int? CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public string IsActive { get; set; }
        public bool IsSync { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Image {  get; set; }
        public int Index { get; set; }
        public bool IsPublic { get; set; }
        public List<TranslationCategoryModel> Translations { get; set; }
    }
}