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
    public class ClientExpenseModel
    {
        [DataMember]
        [Required]
        public ExpenseModel Expense { get; set; }

        [DataMember]
        [Required]
        public List<int> Participants { get; set; }

        public ClientExpenseModel()
        {
            Participants = new List<int>();
        }
    }
}
