using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class OrderAlacarteModel
    {
        public POSHeaderModel POSHeader { get; set; }
        public TransactionAlacarteModel Transaction { get; set; }
        public List<POSDetailModel> POSDetails { get; set; }
        public MsgModel Msg { get; set; }
    }
}