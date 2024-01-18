using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class PatientModel
    {
        public int? PatientID { get; set; }
        public string VisitCode { get; set; }
        public string HN { get; set; }
        public string BedCode { get; set; }
        public string Ward {  get; set; }
        public string PatientFullName { get; set; }
        public DateTime? DoB { get; set; }
        public string Nationality { get; set; }
        public string PrimaryDoctor { get; set; }
        public DateTime? FastingFrom { get; set; }
        public DateTime? FastingTo { get; set;}
        public int? LengthOfStay { get; set; }
        public string PreviousBed { get; set; }
        public string MovedToBed {  get; set; }
        public DateTime? DoNotOrderFrom { get; set; }
        public DateTime? DoNotOrderTo { get; set; } 
        public bool? IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DietaryPropertieModel DietaryPropertieModels { get; set; }
        public DateTime? DischargeDate { get; set; }
        public DateTime? AdmitDate { get; set; }
    }
}