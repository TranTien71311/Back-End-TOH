using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class RoomModel
    {
        public int? Index { get; set; }
        public string RoomID {  get; set; }
        public string RoomCode { get; set; }
        public string WardID { get; set; }
        public string RoomNameEn { get; set; }
        public string RoomNameVn { get; set; }
        public bool? IsActive {  get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}