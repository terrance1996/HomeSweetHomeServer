using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HomeSweetHomeServer.Models
{
    public class ClientHouseworkModel
    {
        [Required]
        public HouseworkModel Housework { get; set; }

        [Required]
        public int FriendId { get; set; }

        public ClientHouseworkModel()
        { }

        public ClientHouseworkModel(HouseworkModel housework, int friendId)
        {
            Housework = housework;
            FriendId = friendId;
        }
    }
}
