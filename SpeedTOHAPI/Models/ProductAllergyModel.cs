using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class ProductAllergyModel
    {
        public int? ProductAllergyID { get; set; }
        public int? DietCode { get; set; }
        public int? ProductCode { get; set; }
        public bool? IsActive { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedDate { get; set; }
    }
}