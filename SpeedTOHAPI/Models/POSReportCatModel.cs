using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class POSReportCatModel
    {
        public int? ReportCatID { get; set; }
        public int? ReportNo {  get; set; }
        public string ReportName { get; set; }
        public string Image {  get; set; }
        public int? SummaryNum { get; set;}
        public string SummaryName { get; set; }
        public bool? Course { get; set; }
        public int? Index { get; set; }
        public bool? IsPublic {  get; set; }
        public bool? IsActive { get; set;}
        public DateTime? DateCreated {  get; set; }
        public DateTime? DateModified {  get; set; }
        public List<TranslationPOSReportCatModel> Translations { get; set; }
    }
}