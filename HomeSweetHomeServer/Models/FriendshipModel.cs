using System;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeSweetHomeServer.Models
{
    //Keeps friendship informations
    [Serializable]
    [DataContract]
    public class FriendshipModel
    {
        [Key]
        [DataMember]
        public int Id { get; set; }

        [ForeignKey("User1Id")]
        public UserModel User1 { get; set; }
        
        [ForeignKey("User2Id")]
        public UserModel User2 { get; set; }

        [DataMember]
        public double Debt { get; set; }

        public FriendshipModel(UserModel user1, UserModel user2, double debt)
        {
            User1 = user1;
            User2 = user2;
            Debt = debt;
        }

        public FriendshipModel()
        {

        }
    }
}
