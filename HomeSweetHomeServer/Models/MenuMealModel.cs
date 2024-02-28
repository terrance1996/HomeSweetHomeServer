using System;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeSweetHomeServer.Models
{
    //Makes a brigde between home menu and home meal
    [Serializable]
    [DataContract]
    public class MenuMealModel
    {
        [Key]
        [DataMember]
        public int Id { get; set; }

        [ForeignKey("MenuId")]
        public MenuModel Menu { get; set; }

        [ForeignKey("MealId")]
        public MealModel Meal { get; set; }

        public MenuMealModel()
        {

        }

        public MenuMealModel(MenuModel menu, MealModel meal)
        {
            Menu = menu;
            Meal = meal;
        }
    }
}
