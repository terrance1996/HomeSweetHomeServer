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
    [Route("api/ShoppingList")]
    [Authorize]
    public class ShoppingListController : Controller
    {
        IJwtTokenService _jwtTokenService;
        IShoppingListService _shoppingListService;

        public ShoppingListController(IJwtTokenService jwtTokenService, IShoppingListService shoppingListService)
        {
            _jwtTokenService = jwtTokenService;
            _shoppingListService = shoppingListService;
        }

        //Synchronizes clients shopping list
        [HttpGet("Synchronize", Name = "SynchronizeShoppingList")]
        public async Task<IActionResult> Synchronize()
        {
            string token = Request.Headers["Authorization"].ToString().Substring("Bearer ".Length).Trim();
            UserModel user = await _jwtTokenService.GetUserFromTokenStrAsync(token);

            ShoppingListModel res = await _shoppingListService.SynchronizeShoppingListAsync(user);

            return Ok(res);
        }

        //Updates shopping list
        [HttpPost("UpdateShoppingList", Name = "UpdateShoppingList")]
        public async Task<IActionResult> UpdateShoppingList([FromBody] ShoppingListModel shoppingList)
        {
            string token = Request.Headers["Authorization"].ToString().Substring("Bearer ".Length).Trim();
            UserModel user = await _jwtTokenService.GetUserFromTokenStrAsync(token);

            await _shoppingListService.UpdateShoppingListAsync(user, shoppingList);

            return Ok();
        }

        //Sends notification to all friends for shopping
        [HttpGet("SendNotification", Name = "SendNotification")]
        public async Task<IActionResult> SendNotification()
        {
            string token = Request.Headers["Authorization"].ToString().Substring("Bearer ".Length).Trim();
            UserModel user = await _jwtTokenService.GetUserFromTokenStrAsync(token);

            await _shoppingListService.SendNotification(user);

            return Ok();
        }
    }
}
