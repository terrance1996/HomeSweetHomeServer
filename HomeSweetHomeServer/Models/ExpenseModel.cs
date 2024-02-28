using System;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeSweetHomeServer.Models
{
    //Expense information
    [Serializable]
    [DataContract]
    public class ExpenseModel
    {
        [Key]
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        [Required]
        public int EType { get; set; }

        [DataMember]
        [Required]
        public double Cost { get; set; }

        [ForeignKey("AuthorId")]
        public UserModel Author { get; set; }

        [ForeignKey("HomeId")]
        public HomeModel Home { get; set; }
        
        [DataMember]
        public DateTime LastUpdated { get; set; }

        [DataMember]
        [Required]
        public string Title { get; set; }

        [DataMember]
        public string Content { get; set; }

        public ExpenseModel()
        {

        }

        public ExpenseModel(int eType, double cost, UserModel author, HomeModel home, DateTime lastUpdated, string title, string content)
        {
            EType = eType;
            Cost = cost;
            Author = author;
            Home = home;
            LastUpdated = lastUpdated;
            Title = title;
            Content = content;
        }
    }
   
    public enum ExpenseType
    {
        HouseRent = 1,
        Bill = 2,
        Shopping = 3,
        Lend = 4,
        Borrow = 6,
        Others = 5
    }
}
