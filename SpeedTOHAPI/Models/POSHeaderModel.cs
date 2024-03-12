using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class POSHeaderModel
    {
        public int TRANSACT { set; get; }
        public int TABLENUM { set; get; }
        public DateTime TIMESTART { set; get; }
        public DateTime TIMEEND { set; get; }
        public int NUMCUST { set; get; }
        public double TAX1 { set; get; }
        public double TAX2 { set; get; }
        public double TAX3 { set; get; }
        public double TAX4 { set; get; }
        public double TAX5 { set; get; }
        public double TAX1ABLE { set; get; }
        public double TAX2ABLE { set; get; }
        public double TAX3ABLE { set; get; }
        public double TAX4ABLE { set; get; }
        public double TAX5ABLE { set; get; }
        public double NETTOTAL { set; get; }
        public int WHOSTART { set; get; }
        public int WHOCLOSE { set; get; }
        public int ISSPLIT { set; get; }
        public int SALETYPEINDEX { set; get; }
        public int EXP { set; get; }
        public int WAITINGAUTH { set; get; }
        public int STATNUM { set; get; }
        public int STATUS { set; get; }
        public double FINALTOTAL { set; get; }
        public int StoreNum { set; get; }
        public int PUNCHINDEX { set; get; }
        public double Gratuity { set; get; }
        public DateTime OPENDATE { set; get; }
        public int MemCode { set; get; }
        public double TotalPoints { set; get; }
        public int PointsApplied { set; get; }
        public int UpdateStatus { set; get; }
        public int ISDelivery { set; get; }
        public DateTime ScheduleDate { set; get; }
        public int Tax1Exempt { set; get; }
        public int Tax2Exempt { set; get; }
        public int Tax3Exempt { set; get; }
        public int Tax4Exempt { set; get; }
        public int Tax5Exempt { set; get; }
        public double MEMRATE { set; get; }
        public int MealTime { set; get; }
        public int IsInternet { set; get; }
        public int RevCenter { set; get; }
        public int PunchIdxStart { set; get; }
        public int StatNumStart { set; get; }
        public int SecNum { set; get; }
        public double GratAmount { set; get; }
        public int ShipTo { set; get; }
        public int EnforcedGrat { set; get; }
        public int NumPrintedFinal { set; get; }
        public string RefId { set; get; }
        public int RstOrdNum { set; get; }
        public string LABEL { set; get; }

        public int PaymentStatus { get; set; }
        public int DeliveryStatus { get; set; }
        public int? GETTRANSACT { set; get; }
    }
}