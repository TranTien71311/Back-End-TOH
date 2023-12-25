using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class DietModel
    {
        public int DietID { get; set; }
        public string DietCode { get; set; }
        public int DietType { get; set; }
        public string DietName {  get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsActive {  get; set; }
    }
}