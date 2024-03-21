using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class CloudProductModel
    {
        public int ProductID { get; set; }
        public string GUID { get; set; }
        public string ProductName { get; set; }
        public Nullable<int> SubCategoryID { get; set; }
        public Nullable<double> PriceA { get; set; }
        public Nullable<double> PriceB { get; set; }
        public Nullable<double> PriceC { get; set; }
        public Nullable<double> PriceD { get; set; }
        public Nullable<double> PriceE { get; set; }
        public Nullable<double> PriceF { get; set; }
        public Nullable<double> PriceG { get; set; }
        public Nullable<double> PriceH { get; set; }
        public Nullable<double> PriceI { get; set; }
        public Nullable<double> PriceJ { get; set; }
        public Nullable<int> SizeUp { get; set; }
        public Nullable<int> SizeDown { get; set; }
        public Nullable<int> Tax1 { get; set; }
        public Nullable<int> Tax2 { get; set; }
        public Nullable<int> Tax3 { get; set; }
        public Nullable<int> Tax4 { get; set; }
        public Nullable<int> Tax5 { get; set; }
        public Nullable<int> QuestionID1 { get; set; }
        public Nullable<int> QuestionID2 { get; set; }
        public Nullable<int> QuestionID3 { get; set; }
        public Nullable<int> QuestionID4 { get; set; }
        public Nullable<int> QuestionID5 { get; set; }
        public Nullable<double> ModifyPrice { get; set; }
        public int StoreID { get; set; }
        public Nullable<int> PartnerID { get; set; }
        public string ReferenceCode { get; set; }
        public Nullable<System.DateTime> DateCreated { get; set; }
        public Nullable<System.DateTime> DateModified { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<int> POSSubCategoryID { get; set; }
        public int ProductCode { get; set; }
        public Nullable<double> StandardCost { get; set; }
        public Nullable<bool> ApplyStandardCost { get; set; }
        public Nullable<double> PercentOfFC { get; set; }
        public Nullable<bool> ProductState { get; set; }
        public string ProductExtraField { get; set; }
        public Nullable<int> ManualPrice { get; set; }
        public string VNeseProductName { get; set; }
        public Nullable<int> WarehouseID { get; set; }
        public Nullable<int> RecipeType { get; set; }
    }
}