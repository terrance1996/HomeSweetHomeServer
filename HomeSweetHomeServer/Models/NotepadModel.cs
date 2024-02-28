using System;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeSweetHomeServer.Models
{
    //Notepad information
    [Serializable]
    [DataContract]
    public class NotepadModel
    {
        [Key]
        [DataMember]
        public int Id { get; set; }
        
        [ForeignKey("HomeId")]
        public HomeModel Home { get; set; }

        [Required]
        [DataMember]
        [MinLength(1)]
        [MaxLength(20)]
        public string Title { get; set; }

        [Required]
        [DataMember]
        [MinLength(1)]
        public string Content { get; set; }

        public NotepadModel(int id, HomeModel home, string title, string content)
        {
            Id = id;
            Home = home;
            Title = title;
            Content = content;
        }

        public NotepadModel()
        {

        }
    }
}
