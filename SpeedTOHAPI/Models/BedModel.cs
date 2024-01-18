using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class BedModel
    {
        public int? Index { get; set; }
        public string BedID { get; set; }
        public string BedCode { get; set; }
        public string RoomID { get; set; }
        public string BedName { get; set;}
        public bool? IsActive {  get; set; }
        public DateTime? CreatedDate {  get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}