using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class PaymentModel
    {
        public int? PaymentID { get; set; }
        public string PaymentName {  get; set; }
        public bool? IsActive {  get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}