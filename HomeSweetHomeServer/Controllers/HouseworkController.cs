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
    [Route("api/Housework")]
    [Authorize]
    public class HouseworkController : Controller
    {
        IJwtTokenService _jwtTokenService;
        IHouseworkService _houseworkService;

        public HouseworkController(IJwtTokenService jwtTokenService, IHouseworkService houseworkService)
        {
            _jwtTokenService = jwtTokenService;
            _houseworkService = houseworkService;
        }

        //Synchronizes clients houseworks
        [HttpGet("Synchronize", Name = "SynchronizeHouseworks")]
        public async Task<IActionResult> Synchronize()
        {
            string token = Request.Headers["Authorization"].ToString().Substring("Bearer ".Length).Trim();
            UserModel user = await _jwtTokenService.GetUserFromTokenStrAsync(token);

            List<ClientHouseworkModel> res = await _houseworkService.SynchronizeHouseworksAsync(user);

            return Ok(res);
        }

        //Admin adds a housework assign
        [HttpPost("AddHousework", Name = "AddHousework")]
        public async Task<IActionResult> AddHousework([FromBody] ClientHouseworkModel clientHousework)
        {
            string token = Request.Headers["Authorization"].ToString().Substring("Bearer ".Length).Trim();
            UserModel user = await _jwtTokenService.GetUserFromTokenStrAsync(token);

            HouseworkModel housework = clientHousework.Housework;
            int friendId = clientHousework.FriendId;

            await _houseworkService.AddHouseworkAsync(user, housework, friendId);

            return Ok();
        }

        //Admin deletes a housework assign
        [HttpGet("DeleteHousework", Name = "DeleteHousework")]
        public async Task<IActionResult> DeleteHousework([FromQuery] int houseworkId)
        {
            string token = Request.Headers["Authorization"].ToString().Substring("Bearer ".Length).Trim();
            UserModel user = await _jwtTokenService.GetUserFromTokenStrAsync(token);

            await _houseworkService.DeleteHouseworkAsync(user, houseworkId);

            return Ok();
        }

        //Admin updates a housework assign
        [HttpPost("UpdateHousework", Name = "UpdateHousework")]
        public async Task<IActionResult> UpdateHousework([FromBody] ClientHouseworkModel clientHousework)
        {
            string token = Request.Headers["Authorization"].ToString().Substring("Bearer ".Length).Trim();
            UserModel user = await _jwtTokenService.GetUserFromTokenStrAsync(token);

            HouseworkModel housework = clientHousework.Housework;
            int friendId = clientHousework.FriendId;

            await _houseworkService.UpdateHouseworkAsync(user, housework, friendId);

            return Ok();
        }
    }
}
