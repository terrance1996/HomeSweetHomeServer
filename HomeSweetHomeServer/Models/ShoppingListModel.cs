using System;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeSweetHomeServer.Models
{
    //Shopping list information
    [Serializable]
    [DataContract]
    public class ShoppingListModel
    {
        [Key]
        [DataMember]
        public int Id { get; set; }
        
        [ForeignKey("HomeId")]
        public HomeModel Home { get; set; }

        [DataMember]
        [Required]
        public string List { get; set; }

        [DataMember]
        [Required]
        public string Status { get; set; }

        public ShoppingListModel()
        {
        }

        public ShoppingListModel(HomeModel home, string list = "", string status = "")
        {
            Home = home;
            List = list;
            Status = status;
        }
    }
}
