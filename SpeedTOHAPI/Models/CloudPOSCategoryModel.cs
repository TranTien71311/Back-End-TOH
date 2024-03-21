using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class CloudPOSCategoryModel
    {
        public int POSCategoryID { get; set; }
        public string GUID { get; set; }
        public string CategoryName { get; set; }
        public Nullable<int> PartnerID { get; set; }
        public string ReferenceCode { get; set; }
        public Nullable<System.DateTime> DateCreated { get; set; }
        public Nullable<System.DateTime> DateModified { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public int StoreID { get; set; }
    }
}