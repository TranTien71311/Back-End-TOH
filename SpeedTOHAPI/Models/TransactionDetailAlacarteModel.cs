using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class TransactionDetailAlacarteModel
    {
        public int? TransactionDetailID { get; set; }
        public int? TransactionCode { get; set; }
        public int? ProductNum { get; set; }
        public int? Quantity {  get; set; }
        public double? Price {  get; set; }
        public double? TotalTax { get; set; }
        public bool? IsActive {  get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}