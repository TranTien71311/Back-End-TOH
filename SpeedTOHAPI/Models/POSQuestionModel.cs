using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class POSQuestionModel
    {
        public int? QuestionID {  get; set; }
        public int? OptionIndex {  get; set; }
        public string Question { get; set; }
        public string Descript {  get; set; }
        public int? Forced {  get; set; }
        public int? NumChoice {  get; set; }
        public int? Allowmulti { get; set; }
        public bool? IsActive {  get; set; }
        public DateTime? DateCreated {  get; set; }
        public DateTime? DateModified { get; set;}
        public List<TranslationQuestionModel> Translations { get; set; }    
    }
}