using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class POSDetailModel
    {
        public int UNIQUEID { set; get; }
        public int TRANSACT { set; get; }
        public int PRODNUM { set; get; }
        public int WHOORDER { set; get; }
        public int WHOAUTH { set; get; }
        public double COSTEACH { set; get; }
        public double QUAN { set; get; }
        public DateTime TIMEORD { set; get; }
        public int PRINTLOC { set; get; }
        public int SEATNUM { set; get; }
        public int Minutes { set; get; }
        public int NOTAX { set; get; }
        public int HOWORDERED { set; get; }
        public int STATUS { set; get; }
        public int NEXTPOS { set; get; }
        public int PRIORPOS { set; get; }
        public int RECPOS { set; get; }
        public int PRODTYPE { set; get; }
        public int ApplyTax1 { set; get; }
        public int Applytax2 { set; get; }
        public int Applytax3 { set; get; }
        public int Applytax4 { set; get; }
        public int Applytax5 { set; get; }
        public int ReduceInventory { set; get; }
        public int StoreNum { set; get; }
        public int STATNUM { set; get; }
        public double RecipeCostEach { set; get; }
        public DateTime OpenDate { set; get; }
        public int MealTime { set; get; }
        public string LineDes { set; get; }
        public int REVCENTER { set; get; }
        public int MasterItem { set; get; }
        public int QuestionId { set; get; }
        public double OrigCostEach { set; get; }
        public double NetCostEach { set; get; }
        public double Discount { set; get; }
        public int UpdateStatus { set; get; }
        public int GratExempt { set; get; }
        public string AuthCode { set; get; }
        public string DESCRIPT { set; get; }
    }
}