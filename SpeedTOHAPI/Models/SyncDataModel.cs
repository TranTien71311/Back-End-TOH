using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class SyncDataModel
    {
        public int? SyncID { get; set; }
        public string SyncName { get; set; }
        public string APIGetLink { get; set; }
        public string APIPostLink { get; set;}
        public string SyncValue { get; set; }
        public bool? IsActive {  get; set; }
        public DateTime? CreatedDate {  get; set; }
        public DateTime? ModifiedDate {  get; set; }
    }
}