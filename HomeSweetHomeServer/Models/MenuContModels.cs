using System;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace HomeSweetHomeServer.Models
{
    //Client request's expense model
    [Serializable]
    [DataContract]
    public class ClientMenuModel
    {
        [DataMember]
        [Required]
        public MenuModel Menu { get; set; }

        [DataMember]
        [Required]
        public List<int> MealIds { get; set; }

        public ClientMenuModel()
        {
            MealIds = new List<int>();
        }
    }
}
