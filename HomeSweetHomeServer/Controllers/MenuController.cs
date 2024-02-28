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
    [Route("api/Menu")]
    [Authorize]
    public class MenuController : Controller
    {
        IJwtTokenService _jwtTokenService;
        IMenuService _menuService;

        public MenuController(IJwtTokenService jwtTokenService, IMenuService menuService)
        {
            _jwtTokenService = jwtTokenService;
            _menuService = menuService;
        }

        //User synchronizes home menus
        [HttpGet("SynchronizeMenus", Name = "SynchronizeMenus")]
        public async Task<IActionResult> SynchronizeMenus()
        {
            string token = Request.Headers["Authorization"].ToString().Substring("Bearer ".Length).Trim();
            UserModel user = await _jwtTokenService.GetUserFromTokenStrAsync(token);

            List<ClientMenuModel> res = await _menuService.SynchronizeMenusAsync(user);

            return Ok(res);
        }

        //User adds menu
        [HttpPost("AddMenu", Name = "AddMenu")]
        public async Task<IActionResult> AddMenu([FromBody]ClientMenuModel clientMenu)
        {
            string token = Request.Headers["Authorization"].ToString().Substring("Bearer ".Length).Trim();
            UserModel user = await _jwtTokenService.GetUserFromTokenStrAsync(token);

            MenuModel menu = clientMenu.Menu;
            List<int> mealIds = clientMenu.MealIds;

            await _menuService.AddMenuAsync(user, menu, mealIds);

            return Ok();
        }

        //User updates menu
        [HttpPost("UpdateMenu", Name = "UpdateMenu")]
        public async Task<IActionResult> UpdateMenu([FromBody]ClientMenuModel clientMenu)
        {
            string token = Request.Headers["Authorization"].ToString().Substring("Bearer ".Length).Trim();
            UserModel user = await _jwtTokenService.GetUserFromTokenStrAsync(token);

            MenuModel menu = clientMenu.Menu;
            List<int> mealIds = clientMenu.MealIds;

            await _menuService.UpdateMenuAsync(user, menu, mealIds);

            return Ok();
        }

        //User deletes menu
        [HttpGet("DeleteMenu", Name = "DeleteMenu")]
        public async Task<IActionResult> DeleteMenu([FromQuery]int menuId)
        {
            string token = Request.Headers["Authorization"].ToString().Substring("Bearer ".Length).Trim();
            UserModel user = await _jwtTokenService.GetUserFromTokenStrAsync(token);

            await _menuService.DeleteMenuAsync(user, menuId);

            return Ok();
        }

        //User synchronizes home meals
        [HttpGet("SynchronizeMeals", Name = "SynchronizeMeals")]
        public async Task<IActionResult> SynchronizeMeals()
        {
            string token = Request.Headers["Authorization"].ToString().Substring("Bearer ".Length).Trim();
            UserModel user = await _jwtTokenService.GetUserFromTokenStrAsync(token);

            List<MealModel> res = await _menuService.SynchronizeMealsAsync(user);

            return Ok(res);
        }

        //User adds meal
        [HttpPost("AddMeal", Name = "AddMeal")]
        public async Task<IActionResult> AddMeal([FromBody]MealModel meal)
        {
            string token = Request.Headers["Authorization"].ToString().Substring("Bearer ".Length).Trim();
            UserModel user = await _jwtTokenService.GetUserFromTokenStrAsync(token);

            meal.Name = meal.Name.ToLower();

            await _menuService.AddMealAsync(user, meal);

            return Ok();
        }
        
        //User updates meal
        [HttpPost("UpdateMeal", Name = "UpdateMeal")]
        public async Task<IActionResult> UpdateMeal([FromBody]MealModel meal)
        {
            string token = Request.Headers["Authorization"].ToString().Substring("Bearer ".Length).Trim();
            UserModel user = await _jwtTokenService.GetUserFromTokenStrAsync(token);

            meal.Name = meal.Name.ToLower();

            await _menuService.UpdateMealAsync(user, meal);

            return Ok();
        }

        //User deletes meal
        [HttpGet("DeleteMeal", Name = "DeleteMeal")]
        public async Task<IActionResult> DeleteMeal([FromQuery]int mealId)
        {
            string token = Request.Headers["Authorization"].ToString().Substring("Bearer ".Length).Trim();
            UserModel user = await _jwtTokenService.GetUserFromTokenStrAsync(token);

            await _menuService.DeleteMealAsync(user, mealId);

            return Ok();
        }
    }
}
