using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class UpdateDatabaseModel
    {
        public int? UpdateID {  get; set; }
        public string UpdateName { get; set; }
        public string UpdateQuery { get; set; }
        public int? UpdateStatus { get; set; }
        public bool? IsActive {  get; set; }
        public DateTime? DateCreated {  get; set; }
        public DateTime? DateModified {  get; set; }
    }
}