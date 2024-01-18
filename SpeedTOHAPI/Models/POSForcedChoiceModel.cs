using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class POSForcedChoiceModel
    {
        public int? ForcedChoiceID {  get; set; }
        public int? UniqueID {  get; set; }
        public int? OptionIndex { get; set; }
        public string Question { get; set; }
        public string Descript { get; set; }
        public int? Sequence { get; set; }
        public int? ChoiceProductNum {  get; set; }
        public string ChoiceProductName { get; set; }
        public double? Price { get; set; }
        public bool? IsActive {  get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified {  get; set; }
        public List<TranslationPOSForceChoiceModel> Translations { get; set; }
    }
}