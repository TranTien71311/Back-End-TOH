using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class CloudStoreModel
    {
        public int StoreID { get; set; }
        public string GUID { get; set; }
        public string StoreName { get; set; }
        public Nullable<int> PointApplyBy { get; set; }
        public Nullable<double> PerAmount { get; set; }
        public Nullable<int> NoOfPoints { get; set; }
        public Nullable<bool> RoundPoints { get; set; }
        public Nullable<int> PartnerID { get; set; }
        public string ReferenceCode { get; set; }
        public Nullable<System.DateTime> DateCreated { get; set; }
        public Nullable<System.DateTime> DateModified { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public string Email { get; set; }
        public Nullable<int> WarehouseCentralID { get; set; }
        public Nullable<int> StockLevelGroupID { get; set; }
        public Nullable<bool> IncludeTax { get; set; }
        public Nullable<bool> SkipReviewPO { get; set; }
        public string ValidOpenTime { get; set; }
        public string EmailBookingSuccess { get; set; }
        public string EmailBookingConfirm { get; set; }
        public string EmailBookingCancel { get; set; }
        public string EmailRequestConfirmationBooking { get; set; }
        public Nullable<int> StoreNum { get; set; }
        public Nullable<bool> IncludeVAT { get; set; }
        public Nullable<bool> IncludeSVC { get; set; }
        public Nullable<double> VATRate { get; set; }
        public Nullable<double> SVCRate { get; set; }
        public Nullable<bool> VATApplyTax1 { get; set; }
        public Nullable<bool> VATApplyTax2 { get; set; }
        public Nullable<bool> VATApplyTax3 { get; set; }
        public Nullable<bool> VATApplyTax4 { get; set; }
        public Nullable<bool> VATApplyTax5 { get; set; }
        public Nullable<bool> SVCApplyTax1 { get; set; }
        public Nullable<bool> SVCApplyTax2 { get; set; }
        public Nullable<bool> SVCApplyTax3 { get; set; }
        public Nullable<bool> SVCApplyTax4 { get; set; }
        public Nullable<bool> SVCApplyTax5 { get; set; }
        public string Address { get; set; }
        public string TaxCode { get; set; }
        public string EmailBookingReminder { get; set; }
        public Nullable<int> ReminderMinute { get; set; }
    }
}