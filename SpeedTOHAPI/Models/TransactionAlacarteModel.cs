using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class TransactionAlacarteModel
    {
        public int? TransactionID { get; set; }
        public int? TransactionCode { get; set; }
        public DateTime? TimeOrder { get; set; }
        public DateTime? OpenDate { get; set; }
        public int? UserOrder { get; set; }
        public int? PatientID { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? IsActive { get; set; }
        public int? PaymentID { get; set; }
    }
}