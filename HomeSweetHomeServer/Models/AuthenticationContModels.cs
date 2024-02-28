using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

//Models for action parameters at authentication controller 
namespace HomeSweetHomeServer.Models
{
    //Login information
    public class LoginModel
    {
        [Required]
        [StringLength(20)]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string DeviceId { get; set; }
    }

    //Verification code information
    public class VerificationCodeModel
    {
        [Required]
        public string VerificationCode { get; set; }

        [Required]
        public int UserId { get; set; }
    }

    //Registiration informations
    public class RegistrationModel
    {
        [Required]
        [StringLength(20)]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        [StringLength(20)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(20)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
        
        [DataType(DataType.DateTime)]
        public DateTime RegistrationDate { get; set; }
    }

    //Forgot password information
    public class ForgotPasswordModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string VerificationCode { get; set; }

        [Required]
        public string NewPassword { get; set; }
    }

    //Basic user informations
    [Serializable]
    [DataContract]
    public class UserBaseModel
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public int Position { get; set; }

        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public double Debt { get; set; }

        public UserBaseModel()
        {

        }

        public UserBaseModel(int id, string username, int position, string firstName, string lastName, double debt)
        {
            Id = id;
            Username = username;
            Position = position;
            FirstName = firstName;
            LastName = lastName;
            Debt = debt;
        }
    }

    //User's full information
    [Serializable]
    [DataContract]
    public class UserFullInformationModel
    {
        [DataMember]
        public UserBaseModel User { get; set; }

        [DataMember]
        public string Token { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string PhoneNumber { get; set; }

        [DataMember]
        public string HomeName { get; set; }

        [DataMember]
        public int NumberOfFriends { get; set; }

        [DataMember]
        public List<UserBaseModel> Friends { get; set; }

        public UserFullInformationModel()
        {
            User = new UserBaseModel();
            Friends = new List<UserBaseModel>();
        }
    }
}
