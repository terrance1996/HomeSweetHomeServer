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
    public class ShoppingListService : IShoppingListService
    {
        IShoppingListRepository _shoppingListRepository;
        IUserRepository _userRepository;
        IFCMService _fcmService;
        IHomeRepository _homeRepository;

        public ShoppingListService(IShoppingListRepository shoppingListRepository,
                                   IUserRepository userRepository,
                                   IFCMService fcmService,
                                   IHomeRepository homeRepository)
        {
            _shoppingListRepository = shoppingListRepository;
            _userRepository = userRepository;
            _fcmService = fcmService;
            _homeRepository = homeRepository;
        }

        //Synchronizes clients shopping list
        public async Task<ShoppingListModel> SynchronizeShoppingListAsync(UserModel user)
        {
            if(user.Position == (int)UserPosition.HasNotHome)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Home Not Exist", "User is not member of a home");
                errors.Throw();
            }
            
            user = await _userRepository.GetByIdAsync(user.Id, true);

            return await _shoppingListRepository.GetShoppingListByHomeIdAsync(user.Home.Id);
        }

        //Updates shopping list
        public async Task UpdateShoppingListAsync(UserModel user, ShoppingListModel shoppingList)
        {
            if (user.Position == (int)UserPosition.HasNotHome)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Home Not Exist", "User is not member of a home");
                errors.Throw();
            }
            
            user = await _userRepository.GetByIdAsync(user.Id, true);
            ShoppingListModel old = await _shoppingListRepository.GetShoppingListByHomeIdAsync(user.Home.Id, true);

            if (old == null)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Shopping List Not Exist", "Shopping list is not exist");
                errors.Throw();
            }
            
            old.List = shoppingList.List;
            old.Status = shoppingList.Status;

            _shoppingListRepository.Update(old);

            foreach (var friend in user.Home.Users)
            {
                FCMModel fcm = new FCMModel(friend.DeviceId, type: "ShoppingListUpdate");
                fcm.data.Add("UpdatedShoppingList", old);
                await _fcmService.SendFCMAsync(fcm);
            }
        }

        //Sends notification to all friends for shopping
        public async Task SendNotification(UserModel user)
        {
            if (user.Position == (int)UserPosition.HasNotHome)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Home Not Exist", "User is not member of a home");
                errors.Throw();
            }

            user = await _userRepository.GetByIdAsync(user.Id, true);
            HomeModel home = await _homeRepository.GetByIdAsync(user.Home.Id, true);

            foreach(var f in home.Users)
            {
                if (f.Id == user.Id)
                    continue;

                FCMModel fcm = new FCMModel(f.DeviceId, new Dictionary<string, object>());
                fcm.notification.Add("title", "Alışveriş Talebi");
                fcm.notification.Add("body", "Alışveriş listesindeki ürünlerin alınması isteniyor.");
                await _fcmService.SendFCMAsync(fcm);
            }
        }
    }
}
