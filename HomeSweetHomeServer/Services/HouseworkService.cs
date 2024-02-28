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
    public class HouseworkService : IHouseworkService
    {
        IFCMService _fcmService;
        IUserRepository _userRepository;
        IHomeRepository _homeRepository;
        IHouseworkRepository _houseworkRepository;

        public HouseworkService(IFCMService fcmService,
                                  IUserRepository userRepository,
                                  IHomeRepository homeRepository,
                                  IHouseworkRepository houseworkRepository)
        {
            _fcmService = fcmService;
            _userRepository = userRepository;
            _homeRepository = homeRepository;
            _houseworkRepository = houseworkRepository;
        }

        //Synchronizes clients houseworks
        public async Task<List<ClientHouseworkModel>> SynchronizeHouseworksAsync(UserModel user)
        {
            if (user.Position == (int)UserPosition.HasNotHome)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Home Not Exist", "User is not member of a home");
                errors.Throw();
            }

            user = await _userRepository.GetByIdAsync(user.Id, true);

            //Gets all home houseworks
            List<HouseworkModel> homeAllHouseworks = await _houseworkRepository.GetAllHomeHouseworksAsync(user.Home.Id, true);

            List<ClientHouseworkModel> clientHouseworks = new List<ClientHouseworkModel>();

            foreach(var hw in homeAllHouseworks)
                clientHouseworks.Add(new ClientHouseworkModel(hw, hw.User.Id));

            return clientHouseworks;
        }

        //Admin adds a housework assign
        public async Task AddHouseworkAsync(UserModel user, HouseworkModel housework, int friendId)
        {
            if (user.Position != (int)UserPosition.Admin)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Authorization Error", "You are not authorized for housework assignment");
                errors.Throw();
            }

            if(housework.Day < 1 || housework.Day > 31)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Housework Day Error", "You can assign a day only  between 1-31");
                errors.Throw();
            }

            user = await _userRepository.GetByIdAsync(user.Id, true);
            HomeModel home = await _homeRepository.GetByIdAsync(user.Home.Id, true);
            UserModel friend = await _userRepository.GetByIdAsync(friendId, true);

            if((friend == null) || (friend.Home != user.Home))
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Friendship Not Found", "Friendship not found for assignment");
                errors.Throw();
            }

            housework.User = friend;
            housework.Home = home;

            _houseworkRepository.Insert(housework);

            //Sends fcm to assigned friend
            FCMModel fcmFriend = new FCMModel(friend.DeviceId, new Dictionary<string, object>());
            fcmFriend.notification.Add("title", "Yeni Ev İşi");
            fcmFriend.notification.Add("body", String.Format("Ayın {0}. günü {1} yapmanız gerekmektedir.", housework.Day, housework.Work));

            await _fcmService.SendFCMAsync(fcmFriend);

            //Sends fcm to all friends
            foreach(var f in home.Users)
            {
                FCMModel fcm = new FCMModel(f.DeviceId, type: "AddHousework");
                fcm.data.Add("Housework", housework);
                fcm.data.Add("FriendId", friendId);

                await _fcmService.SendFCMAsync(fcm);
            }

        }

        //Admin deletes a housework assign
        public async Task DeleteHouseworkAsync(UserModel user, int houseworkId)
        {
            if (user.Position != (int)UserPosition.Admin)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Authorization Error", "You are not authorized for housework assignment");
                errors.Throw();
            }

            user = await _userRepository.GetByIdAsync(user.Id, true);
            HomeModel home = await _homeRepository.GetByIdAsync(user.Home.Id, true);
            HouseworkModel housework = await _houseworkRepository.GetHouseworkByIdAsync(houseworkId, true);

            if((housework == null))
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Housework Not Found", "Housework not found for delete");
                errors.Throw();
            }

            if(housework.Home != user.Home)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Housework Not Belongs Home", "Housework not belongs for this home");
                errors.Throw();
            }
            
            //Sends fcm to assigned friend
            FCMModel fcmFriend = new FCMModel(housework.User.DeviceId, new Dictionary<string, object>());
            fcmFriend.notification.Add("title", "Ev İşi İptal Edildi");
            fcmFriend.notification.Add("body", String.Format("Ayın {0}. yapmanız gereken {1} işi iptal edildi.", housework.Day, housework.Work));

            await _fcmService.SendFCMAsync(fcmFriend);

            _houseworkRepository.Delete(housework);

           
            await _fcmService.SendFCMAsync(fcmFriend);

            //Sends fcm to all friends
            foreach (var f in home.Users)
            {
                FCMModel fcm = new FCMModel(f.DeviceId, type: "DeleteHousework");
                fcm.data.Add("HouseworkId", houseworkId);

                await _fcmService.SendFCMAsync(fcm);
            }
        }

        //Admin updates a housework assign
        public async Task UpdateHouseworkAsync(UserModel user, HouseworkModel housework, int friendId)
        {
            if (user.Position != (int)UserPosition.Admin)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Authorization Error", "You are not authorized for housework assignment");
                errors.Throw();
            }

            if (housework.Day < 1 || housework.Day > 31)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Housework Day Error", "You can assign a day only  between 1-31");
                errors.Throw();
            }

            HouseworkModel oldHousework = await _houseworkRepository.GetHouseworkByIdAsync(housework.Id);
            user = await _userRepository.GetByIdAsync(user.Id, true);
            HomeModel home = await _homeRepository.GetByIdAsync(user.Home.Id, true);
            UserModel newFriend = await _userRepository.GetByIdAsync(friendId, true);

            if (oldHousework == null)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Housework Not Found", "Housework not found for delete");
                errors.Throw();
            }

            if(oldHousework.Home != user.Home)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Housework Not Belongs Home", "Housework not belongs for this home");
                errors.Throw();
            }

            if ((newFriend == null) || (newFriend.Home != user.Home))
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Friendship Not Found", "Friendship not found for assignment");
                errors.Throw();
            }

            oldHousework.Day = housework.Day;
            oldHousework.User = newFriend;
            oldHousework.Work = housework.Work;

            _houseworkRepository.Update(oldHousework);
            
            //Sends fcm to assigned friend
            FCMModel fcmFriend = new FCMModel(housework.User.DeviceId, new Dictionary<string, object>());
            fcmFriend.notification.Add("title", "Ev İşi Güncellendi");
            fcmFriend.notification.Add("body", "Yapmanız gereken bir ev işi güncellendi.");

            await _fcmService.SendFCMAsync(fcmFriend);

            //Sends fcm to all friends
            foreach (var f in home.Users)
            {
                FCMModel fcm = new FCMModel(f.DeviceId, type: "UpdateHousework");
                fcm.data.Add("Housework", housework);
                fcm.data.Add("FriendId", friendId);

                await _fcmService.SendFCMAsync(fcm);
            }
        }
    }
}
