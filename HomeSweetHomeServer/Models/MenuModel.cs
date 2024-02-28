using System;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace HomeSweetHomeServer.Models
{
    //Daily menu information
    [Serializable]
    [DataContract]
    public class MenuModel
    {
        [DataMember]
        [Key]
        public int Id { get; set; }

        [ForeignKey("HomeId")]
        public HomeModel Home { get; set; }

        [DataMember]
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; }
    }
}
