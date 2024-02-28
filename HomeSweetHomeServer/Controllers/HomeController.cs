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
    [Route("api/Home")]
    [Authorize]
    public class HomeController : Controller
    {
        IHomeService _homeService;
        IJwtTokenService _jwtTokenService;
        IAuthenticationService _authenticationService;

        public HomeController(IHomeService homeService, IJwtTokenService jwtTokenService, IAuthenticationService authenticationService)
        {
            _homeService = homeService;
            _jwtTokenService = jwtTokenService;
            _authenticationService = authenticationService;
        }

        //Creates new home from admin
        [HttpPost("CreateNewHome", Name = "CreateNewHome")]
        public async Task<IActionResult> CreateNewHome([FromBody] HomeModel home)
        {
            home.Name = home.Name.ToLower();

            string token = Request.Headers["Authorization"].ToString().Substring("Bearer ".Length).Trim();
            UserModel user = await _jwtTokenService.GetUserFromTokenStrAsync(token);

            await _homeService.CreateNewHomeAsync(user, home);
            return Ok();
        }

        //Requests admin for joining home
        [HttpGet("JoinHomeRequest", Name = "JoinHomeRequest")]
        public async Task<IActionResult> JoinHomeRequest([FromQuery] string joinHomeName)
        {
            joinHomeName = joinHomeName.ToLower();

            string token = Request.Headers["Authorization"].ToString().Substring("Bearer ".Length).Trim();
            UserModel user = await _jwtTokenService.GetUserFromTokenStrAsync(token);

            await _homeService.JoinHomeRequestAsync(user, joinHomeName);

            return Ok();
        }

        //Admin accepts user's request
        [HttpGet("JoinHomeAccept", Name = "JoinHomeAccept")]
        public async Task<IActionResult> JoinHomeAccept([FromQuery] int requesterId, [FromQuery] bool isAccepted)
        {
            string token = Request.Headers["Authorization"].ToString().Substring("Bearer ".Length).Trim();
            UserModel user = await _jwtTokenService.GetUserFromTokenStrAsync(token);

            await _homeService.JoinHomeAcceptAsync(user, requesterId, isAccepted);

            return Ok();
        }

        //Requests user for inviting home
        [HttpGet("InviteHomeRequest", Name = "InviteHomeRequest")]
        public async Task<IActionResult> InviteHomeRequest([FromQuery] string invitedUsername)
        {
            invitedUsername = invitedUsername.ToLower();

            string token = Request.Headers["Authorization"].ToString().Substring("Bearer ".Length).Trim();
            UserModel user = await _jwtTokenService.GetUserFromTokenStrAsync(token);

            await _homeService.InviteHomeRequestAsync(user, invitedUsername);

            return Ok();
        }

        //User accepts admin's request
        [HttpGet("InviteHomeAccept", Name = "InviteHomeAccept")]
        public async Task<IActionResult> InviteHomeAccept([FromQuery] int invitedHomeId, [FromQuery] bool isAccepted)
        {
            string token = Request.Headers["Authorization"].ToString().Substring("Bearer ".Length).Trim();
            UserModel user = await _jwtTokenService.GetUserFromTokenStrAsync(token);

            await _homeService.InviteHomeAcceptAsync(user, invitedHomeId, isAccepted);

            return Ok();
        }

        //User requests to quit home
        [HttpGet("LeaveHome", Name = "LeaveHome")]
        public async Task<IActionResult> LeaveHome([FromQuery] int newAdminId = 0)
        {
            string token = Request.Headers["Authorization"].ToString().Substring("Bearer ".Length).Trim();
            UserModel user = await _jwtTokenService.GetUserFromTokenStrAsync(token);

            await _homeService.LeaveHomeAsync(user, newAdminId);
            return Ok();
        }

        //User requests to quit home
        [HttpGet("BanishFromHome", Name = "BanishFromHome")]
        public async Task<IActionResult> BanishFromHome([FromQuery] int bannedUserId)
        {
            string token = Request.Headers["Authorization"].ToString().Substring("Bearer ".Length).Trim();
            UserModel user = await _jwtTokenService.GetUserFromTokenStrAsync(token);

            await _homeService.BanishFromHomeAsync(user, bannedUserId);
            return Ok();
        }
    }
}
