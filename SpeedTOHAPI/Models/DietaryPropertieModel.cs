using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeedTOHAPI.Models
{
    public class DietaryPropertieModel
    {
        public int DietaryID { get; set; }
        public string VisitCode { get; set; }
        public string HN { get; set; }
        public string FoodTexture { get; set; }
        public string Comments { get; set; }
        public string KitchenCode { get; set; }
        public string PantryCode { get; set; }
        public string SnackCode { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate {  get; set; }
        public DateTime MModifiedDate {  get; set; }
        public List<FoodAllergieModel> FoodAllergies { get; set; }
        public List<MenuTypeModel> MenuTypeModels { get; set; }
    }
}