using System;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace HomeSweetHomeServer.Models
{
    //Keeps home informations
    [Serializable]
    [DataContract]
    public class HomeModel
    {
        [Key]
        [DataMember]
        public int Id { get; set; }
        
        [Required]
        [DataMember]
        public string Name { get; set; }

        [ForeignKey("AdminId")]
        public UserModel Admin { get; set; }

        [DataMember]
        public List<UserModel> Users { get; set; }
    }
}
