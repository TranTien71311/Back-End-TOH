using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class POSProductComboModel
    {
        public int? POSProductComboID {  get; set; }
        public int? ProductComboID { get; set; }
        public int? ProdLinkNum {  get; set; }
        public string ProdLinkName {  get; set; }
        public int? Sequence {  get; set; }
        public int? ProductNum { get; set; }
        public string ProductName { get; set; }
        public double? Price {  get; set; }
        public bool? IsPublic { get; set; }
        public bool? IsActive { get; set;}
        public DateTime? DateCreated {  get; set; }
        public DateTime? DateModified {  get; set; }
        public List<TranslationPOSProductComboModel> Translations { get; set; }
    }
}