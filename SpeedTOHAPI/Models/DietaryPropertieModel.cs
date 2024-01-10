using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class DietaryPropertieModel
    {
        public int DietaryID { get; set; }
        public int PatientID { get; set; }
        public string VisitCode { get; set; }
        public string HN { get; set; }
        public string FoodTexture { get; set; }
        public string Comments { get; set; }
        public string KitchenCode { get; set; }
        public string KitchenName { get; set; }
        public string PantryCode { get; set; }
        public string PantryName { get; set; }
        public string SnackCode { get; set; }
        public string SnackName { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public bool? IsActive { get; set; }
        public DateTime CreatedDate {  get; set; }
        public DateTime ModifiedDate {  get; set; }
        public List<FoodAllergieModel> FoodAllergiesList { get; set; }
        public List<MenuTypeModel> MenuTypes { get; set; }
        public List<string> DietCode { get; set; }
        public List<string> FoodAllergies { get; set; }
    }
}