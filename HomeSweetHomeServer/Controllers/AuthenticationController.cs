using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HomeSweetHomeServer.Models;
using HomeSweetHomeServer.Services;
using Newtonsoft.Json.Serialization;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using HomeSweetHomeServer.Exceptions;
using Newtonsoft.Json;
using System.Web;
using System.Net;

namespace HomeSweetHomeServer.Controllers
{
    [Produces("application/json")]
    [Route("api/Authentication")]
    public class AuthenticationController : Controller
    {
        IAuthenticationService _authenticationService;
        IJwtTokenService _jwtTokenService;
        IFCMService _fcmService;

        public AuthenticationController(IAuthenticationService authenticationService, IJwtTokenService jwtTokenService,IFCMService fcmService)
        {
            _authenticationService = authenticationService;
            _jwtTokenService = jwtTokenService;
            _fcmService = fcmService;
        }

        //Registers user
        [HttpPost("Register", Name = "Register")]
        public async Task<IActionResult> Register([FromBody] RegistrationModel registrationForm)
        {
            registrationForm.RegistrationDate = DateTime.UtcNow;

            registrationForm.Username = registrationForm.Username.ToLower();
            registrationForm.Email = registrationForm.Email.ToLower();

            await _authenticationService.ControlRegisterFormAsync(registrationForm);
            await _authenticationService.RegisterNewUserAsync(registrationForm);

            return Ok();
        }

        //User logins to server and takes JWT for requests
        [HttpPost("Login", Name = "Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            login.Username = login.Username.ToLower();

            UserModel user = await _authenticationService.LoginAsync(login);

            UserFullInformationModel fullInfo = await _authenticationService.GetUserFullInformationAsync(user.Id);
            /*
            if (user.Status == (int)UserStatus.NotValid)
                fullInfo.Token = null;*/

            if (user.Status == (int)UserStatus.NotValid)
                return StatusCode((int)HttpStatusCode.Accepted, fullInfo);
                //return StatusCode((int)HttpStatusCode.Accepted, user.Token);
            else
                return Ok(fullInfo);
        }

        //Requests for send email verification code to email
        [HttpGet("EMailVerification", Name = "EMailVerification")]
        public async Task<IActionResult> EMailVerification([FromQuery] int userId)
        {
            var user = await _authenticationService.GetUserByIdAsync(userId);

            await _authenticationService.SendEmailVerificationCodeToUserAsync(user);

            return Ok();
        }

        //Verifies email
        [HttpPost("VerifyEmail", Name = "VerifyEmail")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerificationCodeModel verificationCode)
        {
            var user = await _authenticationService.GetUserByIdAsync(verificationCode.UserId);

            await _authenticationService.VerifyEmailAsync(user, verificationCode);

            return Ok(user.Token);
        }

        //Requests for forgot password verification code
        [HttpGet("ForgotPassword", Name = "ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromQuery] string email)
        {
            email = email.ToLower();

            UserModel user = await _authenticationService.GetUserByMailAsync(email);

            await _authenticationService.SendForgotPasswordVerificationCodeToUserAsync(user);

            return Ok();
        }

        //Changes password
        [HttpPost("ChangePassword", Name = "ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ForgotPasswordModel forgotPassword)
        {
            forgotPassword.Email = forgotPassword.Email.ToLower();

            UserModel user = await _authenticationService.GetUserByMailAsync(forgotPassword.Email);

            await _authenticationService.ForgotPasswordAsync(user, forgotPassword);

            return Ok();
        }
    }
}
