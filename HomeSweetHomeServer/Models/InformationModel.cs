using System;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace HomeSweetHomeServer.Models
{
    //Keeps knowlegde about a single information
    [Serializable]
    [DataContract]
    public class InformationModel
    {
        [Key]
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string InformationName { get; set; }

        [DataMember]
        public string InformationType { get; set; }
    }
}
