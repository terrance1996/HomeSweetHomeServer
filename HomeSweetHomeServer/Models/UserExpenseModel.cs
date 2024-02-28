using System;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeSweetHomeServer.Models
{
    //Makes a brigde between user and expense
    [Serializable]
    [DataContract]
    public class UserExpenseModel
    {
        [Key]
        [DataMember]
        public int Id { get; set; }

        [ForeignKey("UserId")]
        public UserModel User { get; set; }

        [ForeignKey("ExpenseId")]
        public ExpenseModel Expense { get; set; }

        public UserExpenseModel()
        { }

        public UserExpenseModel(UserModel user, ExpenseModel expense)
        {
            User = user;
            Expense = expense;
        }
    }
}
