using System;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeSweetHomeServer.Models
{
    //Housework information
    [Serializable]
    [DataContract]
    public class HouseworkModel
    {
        [Key]
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        [Required]
        public int Day { get; set; }

        [ForeignKey("HomeId")]
        public HomeModel Home { get; set; }

        [ForeignKey("UserId")]
        public UserModel User { get; set; }

        [DataMember]
        [Required]
        public string Work { get; set; }
    }
}
