using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class FoodAllergieModel
    {
        public int FoodAllergieID {  get; set; }
        public int PatientID {  get; set; }
        public string FoodCode { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsActive { get; set; }
    }
}