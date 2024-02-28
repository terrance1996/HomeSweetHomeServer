using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeSweetHomeServer.Models;
using HomeSweetHomeServer.Repositories;
using HomeSweetHomeServer.Services;
using HomeSweetHomeServer.Exceptions;
using System.Net;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc;

namespace HomeSweetHomeServer.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        IInformationRepository _informationRepository;
        IUserRepository _userRepository;
        IUserInformationRepository _userInformationRepository;
        IConfiguration _config;
        IMailService _mailService;
        IJwtTokenService _jwtTokenService;
        IFriendshipRepository _friendshipRepository;
        IHomeRepository _homeRepository;

        public AuthenticationService(IInformationRepository informationRepository,
                                     IUserRepository userRepository,
                                     IUserInformationRepository userInformationRepository,
                                     IConfiguration config,
                                     IMailService mailService,
                                     IJwtTokenService jwtTokenService,
                                     IFriendshipRepository friendshipRepository,
                                     IHomeRepository homeRepository)
        {
            _informationRepository = informationRepository;
            _userRepository = userRepository;
            _userInformationRepository = userInformationRepository;
            _config = config;
            _mailService = mailService;
            _jwtTokenService = jwtTokenService;
            _friendshipRepository = friendshipRepository;
            _homeRepository = homeRepository;
        }
                
        //Controls registration form is valid
        public async Task ControlRegisterFormAsync(RegistrationModel registrationForm)
        {
            CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
            UserModel authentication = new UserModel();
            authentication.Username = registrationForm.Username;

            var UsernameExist = await _userRepository.GetByUsernameAsync(authentication.Username);
            if (UsernameExist != null)
                errors.AddError("Username", "Username already exist");
            
            var EmailExist = await _userInformationRepository.GetUserInformationByValueAsync(registrationForm.Email);
            if (EmailExist != null)
                errors.AddError("Email", "Email already exist");

            var PhoneNumberExist = await _userInformationRepository.GetUserInformationByValueAsync(registrationForm.PhoneNumber);
            if (PhoneNumberExist != null)
                errors.AddError("PhoneNumber", "Phone number already exist");

            if (errors.Errors.Count != 0)
                errors.Throw();
        }

        //Registers new user
        public async Task RegisterNewUserAsync(RegistrationModel registrationForm)
        {
            UserModel user = new UserModel();
            InformationModel info = new InformationModel();
            UserInformationModel userInformation = new UserInformationModel();

            user.Username = registrationForm.Username;
            user.Password = registrationForm.Password;
            user.Status = (int)UserStatus.NotValid;
            user.Home = null;
            user.Position = (int)UserPosition.HasNotHome;
      
            _userRepository.Insert(user);

            info = await _informationRepository.GetInformationByInformationNameAsync("FirstName");
            userInformation.Information = info;
            userInformation.User = user;
            userInformation.Value = registrationForm.FirstName;
            _userInformationRepository.Insert(userInformation);
            userInformation = null;

            userInformation = new UserInformationModel();
            info = await _informationRepository.GetInformationByInformationNameAsync("LastName");
            userInformation.Information = info;
            userInformation.User = user;
            userInformation.Value = registrationForm.LastName;
            _userInformationRepository.Insert(userInformation);
            userInformation = null;
            
            userInformation = new UserInformationModel();
            info = await _informationRepository.GetInformationByInformationNameAsync("Email");
            userInformation.Information = info;
            userInformation.User = user;
            userInformation.Value = registrationForm.Email;
            _userInformationRepository.Insert(userInformation);
            userInformation = null;

            userInformation = new UserInformationModel();
            info = await _informationRepository.GetInformationByInformationNameAsync("PhoneNumber");
            userInformation.Information = info;
            userInformation.User = user;
            userInformation.Value = registrationForm.PhoneNumber;
            _userInformationRepository.Insert(userInformation);
            userInformation = null;

            userInformation = new UserInformationModel();
            info = await _informationRepository.GetInformationByInformationNameAsync("ReportCount");
            userInformation.Information = info;
            userInformation.User = user;
            userInformation.Value = "0";
            _userInformationRepository.Insert(userInformation);
            userInformation = null;

            userInformation = new UserInformationModel();
            info = await _informationRepository.GetInformationByInformationNameAsync("RegistrationDate");
            userInformation.Information = info;
            userInformation.User = user;
            userInformation.Value = String.Format("{0:u}", registrationForm.RegistrationDate);
            _userInformationRepository.Insert(userInformation);
        }
        
        //Logins user
        public async Task<UserModel> LoginAsync(LoginModel login)
        {
            UserModel user = await _userRepository.GetByUsernameAsync(login.Username);

            if (user == null)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Username", "Username is not registered");
                errors.Throw();
            }

            if(user.Password != login.Password)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Password", "Username and password is not matched");
                errors.Throw();
            }

            if (user.Status == (int)UserStatus.Banned)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("User Is Banned", "User is banned from application");
                errors.Throw();
            }

            user.Token = _jwtTokenService.CreateToken(user);
            user.DeviceId = login.DeviceId;
            _userRepository.Update(user);

            return user;
        }

        //Gets user from user id
        public async Task<UserModel> GetUserByIdAsync(int id, bool include = false)
        {
            UserModel user = await _userRepository.GetByIdAsync(id, include);

            if(user == null)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("User Not Exist", "User is not exist");
                errors.Throw();
            }

            return user;
        }

        //Sends email verification code to user
        public async Task SendEmailVerificationCodeToUserAsync(UserModel user)
        {
            if (user.Status == (int)UserStatus.Valid)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("User Already Verified", "User already verified");
                errors.Throw();
            }

            if (user.Status == (int)UserStatus.Banned)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("User Is Banned", "User is banned from application");
                errors.Throw();
            }

            string verificationCode = new Random().Next(100000, 1000000).ToString();

            Task<UserInformationModel> FirstName = _userInformationRepository.GetUserInformationByIdAsync(user.Id,
                (await _informationRepository.GetInformationByInformationNameAsync("FirstName")).Id);
            Task<UserInformationModel> LastName = _userInformationRepository.GetUserInformationByIdAsync(user.Id,
               (await _informationRepository.GetInformationByInformationNameAsync("LastName")).Id);
            Task<UserInformationModel> Email = _userInformationRepository.GetUserInformationByIdAsync(user.Id,
               (await _informationRepository.GetInformationByInformationNameAsync("Email")).Id);

            InformationModel EmailVerificationCodeInfo = await _informationRepository.GetInformationByInformationNameAsync("EmailVerificationCode");
            InformationModel EmailVerificationCodeGenerateDateInfo = await _informationRepository.GetInformationByInformationNameAsync("EmailVerificationCodeGenerateDate");

            UserInformationModel EmailVerificationCode = await _userInformationRepository.GetUserInformationByIdAsync(user.Id, EmailVerificationCodeInfo.Id);
            UserInformationModel EmailVerificationCodeGenerateDate = await _userInformationRepository.GetUserInformationByIdAsync(user.Id, EmailVerificationCodeGenerateDateInfo.Id);

            //EMail object
            EMailModel mail = new EMailModel();
            mail.FromName = _config["EMailConfiguration:FromName"];
            mail.FromAddress = _config["EMailConfiguration:FromAddress"];
            mail.ToName = (await FirstName).Value +' '+ (await LastName).Value;
            mail.ToAddress = (await Email).Value;
            mail.Subject = "Verification Code";
            mail.Content = "Your email verification code is :" + verificationCode;

            Task sendMail = _mailService.SendMailAsync(mail);

            //First time email verification code send
            if (EmailVerificationCode == null)
            {
                EmailVerificationCode = new UserInformationModel();
                EmailVerificationCode.Information = EmailVerificationCodeInfo;
                EmailVerificationCode.User = user;
                EmailVerificationCode.Value = verificationCode;
                _userInformationRepository.Insert(EmailVerificationCode);

                EmailVerificationCodeGenerateDate = new UserInformationModel();
                EmailVerificationCodeGenerateDate.Information = EmailVerificationCodeGenerateDateInfo;
                EmailVerificationCodeGenerateDate.User = user;
                EmailVerificationCodeGenerateDate.Value = String.Format("{0:u}", DateTime.UtcNow);
                _userInformationRepository.Insert(EmailVerificationCodeGenerateDate);
            }

            //Not first time email verification code send
            else
            {
                EmailVerificationCode.Value = verificationCode;
                _userInformationRepository.Update(EmailVerificationCode);

                EmailVerificationCodeGenerateDate.Value = String.Format("{0:u}", DateTime.UtcNow);
                _userInformationRepository.Update(EmailVerificationCodeGenerateDate);
            }
            await sendMail;
        }

        //Sends forgot password verification code to user
        public async Task SendForgotPasswordVerificationCodeToUserAsync(UserModel user)
        {
            if (user.Status == (int)UserStatus.NotValid)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("None Verified Email", "Your email is not verified");
                errors.Throw();
            }

            if (user.Status == (int)UserStatus.Banned)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("User Is Banned", "User is banned from application");
                errors.Throw();
            }

            string verificationCode = new Random().Next(100000, 1000000).ToString();

            Task<UserInformationModel> FirstName = _userInformationRepository.GetUserInformationByIdAsync(user.Id,
                (await _informationRepository.GetInformationByInformationNameAsync("FirstName")).Id);
            Task<UserInformationModel> LastName = _userInformationRepository.GetUserInformationByIdAsync(user.Id,
               (await _informationRepository.GetInformationByInformationNameAsync("LastName")).Id);
            Task<UserInformationModel> Email = _userInformationRepository.GetUserInformationByIdAsync(user.Id,
               (await _informationRepository.GetInformationByInformationNameAsync("Email")).Id);

            InformationModel ForgotPasswordVerificationCodeInfo = await _informationRepository.GetInformationByInformationNameAsync("ForgotPasswordVerificationCode");
            InformationModel ForgotPasswordVerificationCodeGenerateDateInfo = await _informationRepository.GetInformationByInformationNameAsync("ForgotPasswordVerificationCodeGenerateDate");

            UserInformationModel ForgotPasswordVerificationCode = await _userInformationRepository.GetUserInformationByIdAsync(user.Id, ForgotPasswordVerificationCodeInfo.Id);
            UserInformationModel ForgotPasswordVerificationCodeGenerateDate = await _userInformationRepository.GetUserInformationByIdAsync(user.Id, ForgotPasswordVerificationCodeGenerateDateInfo.Id);

            //EMail object
            EMailModel mail = new EMailModel();
            mail.FromName = _config["EMailConfiguration:FromName"];
            mail.FromAddress = _config["EMailConfiguration:FromAddress"];
            mail.ToName = (await FirstName).Value + ' ' + (await LastName).Value;
            mail.ToAddress = (await Email).Value;
            mail.Subject = "Verification Code";
            mail.Content = "Your password change verification code is :" + verificationCode;

            Task sendMail = _mailService.SendMailAsync(mail);

            //First time verification code send
            if (ForgotPasswordVerificationCode == null)
            {
                ForgotPasswordVerificationCode = new UserInformationModel();
                ForgotPasswordVerificationCode.Information = ForgotPasswordVerificationCodeInfo;
                ForgotPasswordVerificationCode.User = user;
                ForgotPasswordVerificationCode.Value = verificationCode;
                _userInformationRepository.Insert(ForgotPasswordVerificationCode);

                ForgotPasswordVerificationCodeGenerateDate = new UserInformationModel();
                ForgotPasswordVerificationCodeGenerateDate.Information = ForgotPasswordVerificationCodeGenerateDateInfo;
                ForgotPasswordVerificationCodeGenerateDate.User = user;
                ForgotPasswordVerificationCodeGenerateDate.Value = String.Format("{0:u}", DateTime.UtcNow);
                _userInformationRepository.Insert(ForgotPasswordVerificationCodeGenerateDate);
            }

            //Not first time verification code send
            else
            {
                ForgotPasswordVerificationCode.Value = verificationCode;
                _userInformationRepository.Update(ForgotPasswordVerificationCode);

                ForgotPasswordVerificationCodeGenerateDate.Value = String.Format("{0:u}", DateTime.UtcNow);
                _userInformationRepository.Update(ForgotPasswordVerificationCodeGenerateDate);
            }
            await sendMail;
        }

        //Verifies email
        public async Task VerifyEmailAsync(UserModel user, VerificationCodeModel verification)
        {
            if (user.Status == (int)UserStatus.Valid)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("User Already Verified", "User already verified");
                errors.Throw();
            }

            if (user.Status == (int)UserStatus.Banned)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("User Is Banned", "User is banned from application");
                errors.Throw();
            }

            InformationModel EmailVerificationCodeInfo = await _informationRepository.GetInformationByInformationNameAsync("EmailVerificationCode");
            InformationModel EmailVerificationCodeGenerateDateInfo = await _informationRepository.GetInformationByInformationNameAsync("EmailVerificationCodeGenerateDate");

            UserInformationModel EmailVerificationCode = await _userInformationRepository.GetUserInformationByIdAsync(user.Id, EmailVerificationCodeInfo.Id);
            UserInformationModel EmailVerificationCodeGenerateDate = await _userInformationRepository.GetUserInformationByIdAsync(user.Id, EmailVerificationCodeGenerateDateInfo.Id);
            
            //Bad request
            if(EmailVerificationCode == null)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Email Verification Code Not Exist", "There is no verification code which is generated for you");
                errors.Throw();
            }

            //Generated code timed out
            if(String.Format("{0:u}", DateTime.UtcNow.AddMinutes(-15)).CompareTo(EmailVerificationCodeGenerateDate.Value) > 0)
            {
                _userInformationRepository.Delete(EmailVerificationCode);
                _userInformationRepository.Delete(EmailVerificationCodeGenerateDate);

                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Verification Code Timeout", "Verification code timed out, please request another verification code");
                errors.Throw();
            }

            //Verification code accepted
            if (EmailVerificationCode.Value == verification.VerificationCode)
            {
                user.Status = (int)UserStatus.Valid;
                _userRepository.Update(user);

                _userInformationRepository.Delete(EmailVerificationCode);
                _userInformationRepository.Delete(EmailVerificationCodeGenerateDate);

            }
            //Verification code does not matched
            else
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Verification Code", "Verification code does not matched");
                errors.Throw();
            }
        }

        //Changes forgotten password
        public async Task ForgotPasswordAsync(UserModel user, ForgotPasswordModel forgotPassword)
        {
            if (user.Status == (int)UserStatus.NotValid)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("None Verified Email", "Your email is not verified");
                errors.Throw();
            }

            if (user.Status == (int)UserStatus.Banned)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("User Is Banned", "User is banned from application");
                errors.Throw();
            }

            InformationModel ForgotPasswordVerificationCodeInfo = await _informationRepository.GetInformationByInformationNameAsync("ForgotPasswordVerificationCode");
            InformationModel ForgotPasswordVerificationCodeGenerateDateInfo = await _informationRepository.GetInformationByInformationNameAsync("ForgotPasswordVerificationCodeGenerateDate");

            UserInformationModel ForgotPasswordVerificationCode = await _userInformationRepository.GetUserInformationByIdAsync(user.Id, ForgotPasswordVerificationCodeInfo.Id);
            UserInformationModel ForgotPasswordVerificationCodeGenerateDate = await _userInformationRepository.GetUserInformationByIdAsync(user.Id, ForgotPasswordVerificationCodeGenerateDateInfo.Id);

            //Bad request
            if (ForgotPasswordVerificationCode == null)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Email Verification Code Not Exist", "There is no verification code which is generated for you");
                errors.Throw();
            }

            //Generated code timed out
            if (String.Format("{0:u}", DateTime.UtcNow.AddMinutes(-15)).CompareTo(ForgotPasswordVerificationCodeGenerateDate.Value) > 0)
            {
                _userInformationRepository.Delete(ForgotPasswordVerificationCode);
                _userInformationRepository.Delete(ForgotPasswordVerificationCodeGenerateDate);

                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Verification Code Timeout", "Verification code timed out, please request another verification code");
                errors.Throw();
            }

            //Verification code accepted
            if (ForgotPasswordVerificationCode.Value == forgotPassword.VerificationCode)
            {
                user.Password = forgotPassword.NewPassword;
                _userRepository.Update(user);

                _userInformationRepository.Delete(ForgotPasswordVerificationCode);
                _userInformationRepository.Delete(ForgotPasswordVerificationCodeGenerateDate);
            }
            //Verification code does not matched
            else
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Verification Code", "Verification code does not matched");
                errors.Throw();
            }
        }

        //Gets user from email
        public async Task<UserModel> GetUserByMailAsync(string email)
        {
            var Email = await _userInformationRepository.GetUserInformationByValueAsync(email, true);

            if(Email == null)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Email Not Exist", "Email does not exist");
                errors.Throw();
            }

            return Email.User;
        }

        //Gets user full information
        public async Task<UserFullInformationModel> GetUserFullInformationAsync(int id)
        {
            Task<InformationModel> firstNameInfo = _informationRepository.GetInformationByInformationNameAsync("FirstName");
            Task<InformationModel> lastNameInfo = _informationRepository.GetInformationByInformationNameAsync("LastName");
            Task<InformationModel> emailInfo = _informationRepository.GetInformationByInformationNameAsync("Email");
            Task<InformationModel> phoneNumberInfo = _informationRepository.GetInformationByInformationNameAsync("PhoneNumber");

            UserModel user = await GetUserByIdAsync(id);

            Task<UserInformationModel> firstName = _userInformationRepository.GetUserInformationByIdAsync(user.Id, (await firstNameInfo).Id);
            Task<UserInformationModel> lastName = _userInformationRepository.GetUserInformationByIdAsync(user.Id, (await lastNameInfo).Id);
            Task<UserInformationModel> email = _userInformationRepository.GetUserInformationByIdAsync(user.Id, (await emailInfo).Id);
            Task<UserInformationModel> phoneNumber = _userInformationRepository.GetUserInformationByIdAsync(user.Id, (await phoneNumberInfo).Id);
            
            UserFullInformationModel fullInfo = new UserFullInformationModel();

            fullInfo.User.Id = user.Id;
            fullInfo.User.Username = user.Username;
            fullInfo.User.Position = user.Position;
            fullInfo.User.Debt = 0;
            fullInfo.Token = user.Token;

            fullInfo.User.FirstName = (await firstName).Value;
            fullInfo.User.LastName = (await lastName).Value;
            fullInfo.Email = (await email).Value;
            fullInfo.PhoneNumber = (await phoneNumber).Value;

            if (user.Position == (int)UserPosition.HasNotHome)
            {
                fullInfo.NumberOfFriends = 0;
                fullInfo.Friends = null;
                fullInfo.HomeName = null;
            }
            else
            {
                user = await GetUserByIdAsync(id, true);
                HomeModel home = await _homeRepository.GetByIdAsync(user.Home.Id, true);
                
                fullInfo.HomeName = home.Name;
                fullInfo.NumberOfFriends = home.Users.Count - 1;
                
                foreach(var friend in home.Users)
                {
                    if (friend.Equals(user))
                        continue;

                    string friendFirstName = (await _userInformationRepository.GetUserInformationByIdAsync(friend.Id, (await firstNameInfo).Id)).Value;
                    string friendLastName = (await _userInformationRepository.GetUserInformationByIdAsync(friend.Id, (await lastNameInfo).Id)).Value;

                    double debt = 0;
                    FriendshipModel friendship = await _friendshipRepository.GetFriendshipByIdAsync(user.Id, friend.Id);

                    if(friendship == null)
                    {
                        CustomException errors = new CustomException();
                        errors.AddError("Unexpected Error Occured", "Friendship does not exist");
                        errors.Throw();
                    }
                  
                    if (friendship.User1.Id == user.Id)
                        debt = friendship.Debt;
                    else
                        debt = -friendship.Debt;

                    fullInfo.Friends.Add(new UserBaseModel(friend.Id, friend.Username, friend.Position, friendFirstName, friendLastName, debt));
                }
            }

            return fullInfo;
        }
    }
}
