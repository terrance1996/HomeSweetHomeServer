using System;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeSweetHomeServer.Models
{
    //Makes a brigde between user and information
    [Serializable]
    [DataContract]
    public class UserInformationModel
    {
        [Key]
        [DataMember]
        public int Id { get; set; }

        [ForeignKey("UserId")]
        public UserModel User { get; set; }

        [ForeignKey("InformationId")]
        public InformationModel Information { get; set; }

        [DataMember]
        public string Value { get; set; }
    }
}
