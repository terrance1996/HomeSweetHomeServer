using System;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeSweetHomeServer.Models
{
    //BaseClass for any user
    [Serializable]
    [DataContract]
    public class UserModel
    {
        [Key]
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public string Token { get; set; }

        [DataMember]
        public int Status { get; set; }

        [DataMember]
        public int Position { get; set; }

        [ForeignKey("HomeId")]
        public HomeModel Home { get; set; }

        [DataMember]
        public string DeviceId { get; set; }
    }

    public enum UserStatus
    {
        NotValid = 0,
        Valid = 1,
        Banned = 2
    }

    public enum UserPosition
    {
        HasNotHome = 0,
        HasHome = 1,
        Admin = 2
    }
}
