using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class TranslationModel
    {
        public int? TranslationID {  get; set; }
        public string TranslationName { get; set; }
        public bool? IsActive {  get; set; }
        public string Image {  get; set; }
        public string TranslationCode { get; set; }
        public DateTime? DataCreated { get; set; }
        public DateTime? DateModified {  get; set; }
    }
}