using HomeSweetHomeServer.Exceptions;
using HomeSweetHomeServer.Models;
using HomeSweetHomeServer.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace HomeSweetHomeServer.Services
{
    public class HomeService : IHomeService
    {
        IInformationRepository _informationRepository;
        IUserRepository _userRepository;
        IUserInformationRepository _userInformationRepository;
        IConfiguration _config;
        IMailService _mailService;
        IHomeRepository _homeRepository;
        IFCMService _fcmService;
        IFriendshipRepository _friendshipRepository;
        IShoppingListRepository _shoppingListRepository;
        INotepadRepository _notepadRepository;
        IExpenseRepository _expenseRepository;
        IUserExpenseRepository _userExpenseRepository;

        public HomeService(IInformationRepository informationRepository,
                           IUserRepository userRepository,
                           IUserInformationRepository userInformationRepository,
                           IConfiguration config,
                           IMailService mailService,
                           IHomeRepository homeRepository,
                           IFCMService fcmService,
                           IFriendshipRepository friendshipRepository,
                           IShoppingListRepository shoppingListRepository,
                           INotepadRepository notepadRepository,
                           IExpenseRepository expenseRepository,
                           IUserExpenseRepository userExpenseRepository)
        {
            _informationRepository = informationRepository;
            _userRepository = userRepository;
            _userInformationRepository = userInformationRepository;
            _config = config;
            _mailService = mailService;
            _homeRepository = homeRepository;
            _fcmService = fcmService;
            _friendshipRepository = friendshipRepository;
            _shoppingListRepository = shoppingListRepository;
            _notepadRepository = notepadRepository;
            _expenseRepository = expenseRepository;
            _userExpenseRepository = userExpenseRepository;
        }

        //Admin creates the home
        public async Task CreateNewHomeAsync(UserModel user, HomeModel home)
        {
            if(user.Position != (int)UserPosition.HasNotHome)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("User Has Home", "User has home already");
                errors.Throw();
            }

            var isHomeExist = await _homeRepository.GetByHomeNameAsync(home.Name);

            if(isHomeExist != null)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Home Name Exist", "Home name has already exist");
                errors.Throw();
            }
            
            user.Position = (int)UserPosition.Admin;

            home.Admin = user;
            home.Users = new List<UserModel>();
            home.Users.Add(user);
            _homeRepository.Insert(home);

            user.Home = home;
            _userRepository.Update(user);

            _shoppingListRepository.Insert(new ShoppingListModel(home));

        }

        //User requests to admin for joining home
        public async Task JoinHomeRequestAsync(UserModel user, string joinHomeName)
        {
            Task<InformationModel> firstNameInfo = _informationRepository.GetInformationByInformationNameAsync("FirstName");
            Task<InformationModel> lastNameInfo = _informationRepository.GetInformationByInformationNameAsync("LastName");

            if (user.Position != (int)UserPosition.HasNotHome)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("User Has Home", "User has home already");
                errors.Throw();
            }

            var home = await _homeRepository.GetByHomeNameAsync(joinHomeName, true);

            if (home == null)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Home Name Not Exist", "Home name has not exist");
                errors.Throw();
            }

            //User waiting for admin's accept

            UserInformationModel firstName = await _userInformationRepository.GetUserInformationByIdAsync(user.Id, (await firstNameInfo).Id);
            UserInformationModel lastName = await _userInformationRepository.GetUserInformationByIdAsync(user.Id, (await lastNameInfo).Id);
            
            FCMModel fcm = new FCMModel(home.Admin.DeviceId, new Dictionary<string, object>());
            
            fcm.notification.Add("title", "Eve Katılma İsteği");
            fcm.notification.Add("body", String.Format("{0} {1}({2}) evinize katılmak istiyor.", firstName.Value, lastName.Value, user.Username));

            await _fcmService.SendFCMAsync(fcm);

            fcm = new FCMModel(home.Admin.DeviceId, type : "JoinHomeRequest");
           
            fcm.data.Add("RequesterId", user.Id);
            fcm.data.Add("RequesterUsername", user.Username);
            fcm.data.Add("RequesterName", firstName.Value);
            fcm.data.Add("RequesterLastName", lastName.Value);

            await _fcmService.SendFCMAsync(fcm);
        }

        //Admin accepts or rejects user's request
        public async Task JoinHomeAcceptAsync(UserModel user, int requesterId, bool isAccepted)
        {
            Task<UserModel> getAdmin = _userRepository.GetByIdAsync(user.Id, true);
            Task<InformationModel> firstNameInfo = _informationRepository.GetInformationByInformationNameAsync("FirstName");
            Task<InformationModel> lastNameInfo = _informationRepository.GetInformationByInformationNameAsync("LastName");
           
            if (user.Position != (int)UserPosition.Admin)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Authorisation Constraint", "You are not authorized for this request, you must be administrator of home");
                errors.Throw();
            }
            
            UserModel requester = await _userRepository.GetByIdAsync(requesterId);

            if (requester == null)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Invalid User Id", "User not exist");
                errors.Throw();
            }
            
            if(requester.Position != (int)UserPosition.HasNotHome)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Requester Has Home", "Requester has already home");
                errors.Throw();
            }

            user = await getAdmin;

            if (isAccepted == true)
            {
                requester.Position = (int)UserPosition.HasHome;

                UserInformationModel requesterFirstName = await _userInformationRepository.GetUserInformationByIdAsync(requester.Id, (await firstNameInfo).Id);
                UserInformationModel requesterLastName = await _userInformationRepository.GetUserInformationByIdAsync(requester.Id, (await lastNameInfo).Id);

                HomeModel home = await _homeRepository.GetByIdAsync(user.Home.Id, true);

                FCMModel fcmRequester = new FCMModel(requester.DeviceId, new Dictionary<string, object>());

                fcmRequester.notification.Add("title", "Eve Katılma İsteği");
                fcmRequester.notification.Add("body", "Eve katılma isteğiniz ev yöneticisi tarafından kabul edildi.");

                await _fcmService.SendFCMAsync(fcmRequester);

                List<UserBaseModel> friendsBaseModels = new List<UserBaseModel>();
                UserBaseModel requesterBaseModel = new UserBaseModel(requester.Id, requester.Username, requester.Position, requesterFirstName.Value, requesterLastName.Value, 0);

                foreach (var friend in home.Users)
                {
                    FriendshipModel friendship = new FriendshipModel(requester, friend, 0);
                    Task insertFriendship = _friendshipRepository.InsertAsync(friendship);

                    //Sends notification to all friends 
                    FCMModel fcmFriend = new FCMModel(friend.DeviceId, new Dictionary<string, object>());

                    fcmFriend.notification.Add("title", "Yeni Ev Arkadaşı");
                    fcmFriend.notification.Add("body", String.Format("{0} {1}({2}) evinize katıldı.", requesterFirstName.Value, requesterLastName.Value, requester.Username));

                    await _fcmService.SendFCMAsync(fcmFriend);

                    //Sends notification to all friends 
                    fcmFriend = new FCMModel(friend.DeviceId, type : "NewFriend");
                    fcmFriend.data.Add("Friend", requesterBaseModel);

                    await _fcmService.SendFCMAsync(fcmFriend);

                    //Sends all friends to requester
                    UserInformationModel friendFirstName = await _userInformationRepository.GetUserInformationByIdAsync(friend.Id, (await firstNameInfo).Id);
                    UserInformationModel friendLastName = await _userInformationRepository.GetUserInformationByIdAsync(friend.Id, (await lastNameInfo).Id);

                    friendsBaseModels.Add(new UserBaseModel(friend.Id, friend.Username, friend.Position, friendFirstName.Value, friendLastName.Value, 0));

                    await insertFriendship;
                }

                home.Users.Add(requester);
                _homeRepository.Update(home);

                requester.Home = home;
                _userRepository.Update(requester);

                fcmRequester = new FCMModel(requester.DeviceId, type : "AllFriends");
                
                fcmRequester.data.Add("NumberOfFriends", home.Users.Count - 1);
                fcmRequester.data.Add("Friends", friendsBaseModels);
                await _fcmService.SendFCMAsync(fcmRequester);
            }
        }

        //Admin request to user for inviting home
        public async Task InviteHomeRequestAsync(UserModel user, string invitedUsername)
        {
            Task<InformationModel> firstNameInfo = _informationRepository.GetInformationByInformationNameAsync("FirstName");
            Task<InformationModel> lastNameInfo = _informationRepository.GetInformationByInformationNameAsync("LastName");

            if (user.Position != (int)UserPosition.Admin)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Authorisation Constraint", "You are not authorized for this request, you must be administrator of home");
                errors.Throw();
            }

            var home = (await _userRepository.GetByIdAsync(user.Id, true)).Home;

            //Admin waiting for user's accept

            UserModel invitedUser = await _userRepository.GetByUsernameAsync(invitedUsername);

            if(invitedUser == null)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("User Not Exist", "User is not exist");
                errors.Throw();
            }

            if(invitedUser.Position != (int)UserPosition.HasNotHome)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("User Has Home", "You can not invite a user who already has home");
                errors.Throw();
            }

            UserInformationModel firstName = await _userInformationRepository.GetUserInformationByIdAsync(user.Id, (await firstNameInfo).Id);
            UserInformationModel lastName = await _userInformationRepository.GetUserInformationByIdAsync(user.Id, (await lastNameInfo).Id);

            FCMModel fcm = new FCMModel(invitedUser.DeviceId, new Dictionary<string, object>());

            fcm.notification.Add("title", "Eve Katılma Daveti");
            fcm.notification.Add("body", String.Format("{0} {1}({2}) evine katılmanız için davet ediyor.", firstName.Value, lastName.Value, user.Username));

            await _fcmService.SendFCMAsync(fcm);

            fcm = new FCMModel(invitedUser.DeviceId, type : "InviteHomeRequest");            

            fcm.data.Add("InvitedHomeId", home.Id);
            fcm.data.Add("InviterUsername", user.Username);
            fcm.data.Add("InviterFirstName", firstName.Value);
            fcm.data.Add("InviterLastName", lastName.Value);

            await _fcmService.SendFCMAsync(fcm);
        }

        //User accepts or rejects admin's request
        public async Task InviteHomeAcceptAsync(UserModel user, int invitedHomeId, bool isAccepted)
        {
            Task<InformationModel> firstNameInfo = _informationRepository.GetInformationByInformationNameAsync("FirstName");
            Task<InformationModel> lastNameInfo = _informationRepository.GetInformationByInformationNameAsync("LastName");

            if (user.Position != (int)UserPosition.HasNotHome)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("User Has Home", "User can not accept invite requests while already has home");
                errors.Throw();
            }

            HomeModel home = await _homeRepository.GetByIdAsync(invitedHomeId, true);

            if (home == null)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Invalid Home Id", "Home not exist");
                errors.Throw();
            }

            if (isAccepted == true)
            {
                user.Position = (int)UserPosition.HasHome;

                UserInformationModel userFirstName = await _userInformationRepository.GetUserInformationByIdAsync(user.Id, (await firstNameInfo).Id);
                UserInformationModel userLastName = await _userInformationRepository.GetUserInformationByIdAsync(user.Id, (await lastNameInfo).Id);

                List<UserBaseModel> friendsBaseModels = new List<UserBaseModel>();
                UserBaseModel userBaseModel = new UserBaseModel(user.Id, user.Username, user.Position, userFirstName.Value, userLastName.Value, 0);

                FCMModel fcmUser = new FCMModel(user.DeviceId, type: "AllFriends");

                foreach (var friend in home.Users)
                {
                    FriendshipModel friendship = new FriendshipModel(user, friend, 0);
                    Task insertFriendship = _friendshipRepository.InsertAsync(friendship);
                    
                    //Sends notification to all friends 
                    FCMModel fcmFriend = new FCMModel(friend.DeviceId, new Dictionary<string, object>());

                    fcmFriend.notification.Add("title", "Yeni Ev Arkadaşı");
                    fcmFriend.notification.Add("body", String.Format("{0} {1}({2}) evinize katıldı", userFirstName.Value, userLastName.Value, user.Username));
                                        
                    await _fcmService.SendFCMAsync(fcmFriend);

                    //Sends notification to all friends 
                    fcmFriend = new FCMModel(friend.DeviceId, new Dictionary<string, object>(), "NewFriend");
                    fcmFriend.data.Add("Friend", userBaseModel);
                    
                    await _fcmService.SendFCMAsync(fcmFriend);

                    //Sends all friends to requester
                    UserInformationModel friendFirstName = await _userInformationRepository.GetUserInformationByIdAsync(friend.Id, (await firstNameInfo).Id);
                    UserInformationModel friendLastName = await _userInformationRepository.GetUserInformationByIdAsync(friend.Id, (await lastNameInfo).Id);
    
                    friendsBaseModels.Add(new UserBaseModel(friend.Id, friend.Username, friend.Position, friendFirstName.Value, friendLastName.Value, 0));

                    await insertFriendship;
                }

                home.Users.Add(user);
                _homeRepository.Update(home);

                user.Home = home;
                _userRepository.Update(user);

                fcmUser.data.Add("NumberOfFriends", home.Users.Count - 1);
                fcmUser.data.Add("Friends", friendsBaseModels);
                await _fcmService.SendFCMAsync(fcmUser);
            }
        }

        //User gives money to his/her friend
        public async Task TransferMoneyToFriendAsync(UserModel from, UserModel to, double givenMoney)
        { 
            if (to == null)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Friend Not Found", "Friend not found for lend");
                errors.Throw();
            }

            if (from.Position == (int)UserPosition.HasNotHome || to.Position == (int)UserPosition.HasNotHome)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Home Not Exist", "User is not member of a home");
                errors.Throw();
            }

            FriendshipModel friendship = await _friendshipRepository.GetFriendshipByIdAsync(from.Id, to.Id);
            if(friendship == null)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Friendship Not Found", "Friendship not found for lend");
                errors.Throw();
            }

            if(friendship.User1.Id == from.Id)
            {
                friendship.Debt -= givenMoney;
                Task update = _friendshipRepository.UpdateAsync(friendship);

                FCMModel fcmFrom = new FCMModel(from.DeviceId, type: "GiveMoney");
                fcmFrom.data.Add("ToId", to.Id);
                fcmFrom.data.Add("NewDebt", friendship.Debt);
                Task sendFCMFrom = _fcmService.SendFCMAsync(fcmFrom);

                FCMModel fcmTo = new FCMModel(to.DeviceId, type: "TakeMoney");
                fcmTo.data.Add("FromId", from.Id);
                fcmTo.data.Add("NewDebt", -friendship.Debt);

                await _fcmService.SendFCMAsync(fcmTo);
                await sendFCMFrom;
                await update;
            }
            else
            {
                friendship.Debt += givenMoney;
                Task update = _friendshipRepository.UpdateAsync(friendship);

                FCMModel fcmFrom = new FCMModel(from.DeviceId, type: "GiveMoney");
                fcmFrom.data.Add("ToId", to.Id);
                fcmFrom.data.Add("NewDebt", -friendship.Debt);
                Task sendFCMFrom = _fcmService.SendFCMAsync(fcmFrom);

                FCMModel fcmTo = new FCMModel(to.DeviceId, type: "TakeMoney");
                fcmTo.data.Add("FromId", from.Id);
                fcmTo.data.Add("NewDebt", friendship.Debt);

                await _fcmService.SendFCMAsync(fcmTo);
                await sendFCMFrom;
                await update;
            }
        }

        //User request to quit home
        public async Task LeaveHomeAsync(UserModel user, int newAdminId)
        {
            if(user.Position == (int)UserPosition.HasNotHome)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Home Not Exist", "User is not member of a home");
                errors.Throw();
            }

            Task<InformationModel> firstNameInfo = _informationRepository.GetInformationByInformationNameAsync("FirstName");
            Task<InformationModel> lastNameInfo = _informationRepository.GetInformationByInformationNameAsync("LastName");

            user = await _userRepository.GetByIdAsync(user.Id, true);
            HomeModel home = await _homeRepository.GetByIdAsync(user.Home.Id, true);

            UserInformationModel userFirstName = await _userInformationRepository.GetUserInformationByIdAsync(user.Id, (await firstNameInfo).Id);
            UserInformationModel userLastName = await _userInformationRepository.GetUserInformationByIdAsync(user.Id, (await lastNameInfo).Id);

            List<UserExpenseModel> userExpenses = await _userExpenseRepository.GetAllUserExpenseByUserIdAsync(user.Id);

            foreach (var ue in userExpenses)
                _userExpenseRepository.Delete(ue);

            if (home.Users.Count != 1)
            {
                if (user.Position == (int)UserPosition.Admin)
                {
                    UserModel newAdmin = await _userRepository.GetByIdAsync(newAdminId);

                    if (newAdmin == null || newAdmin.Home.Id != user.Home.Id)
                    {
                        CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                        errors.AddError("Friendship Not Found", "Friendship not found for admin assignment");
                        errors.Throw();
                    }
                    newAdmin.Position = (int)UserPosition.Admin;
                    home.Admin = newAdmin;
                }
                
                home.Users.Remove(user);
                user.Home = null;
                user.Position = (int)UserPosition.HasNotHome;
                
                _homeRepository.Update(home);
                _userRepository.Update(user);

                //Home friends notification
                foreach (var u in home.Users)
                {
                    FriendshipModel friendship = await _friendshipRepository.GetFriendshipByIdAsync(user.Id, u.Id);

                    FCMModel fcm = new FCMModel(u.DeviceId, new Dictionary<string, object>());
                    fcm.notification.Add("title", "Evden Ayrılma");

                    if (friendship.User1.Id == user.Id)
                    {
                        if(friendship.Debt > 0)
                        {
                            fcm.notification.Add("body", String.Format("{0} {1} evden ayrılıyor. Alacağınız : {2:c}", userFirstName.Value,
                                                                                                            userLastName.Value,
                                                                                                            friendship.Debt));
                        }
                        else if(friendship.Debt == 0)
                        {
                            fcm.notification.Add("body", String.Format("{0} {1} evden ayrılıyor. Borcunuz veya alacağınız bulunmamaktadır.", userFirstName.Value,
                                                                                                            userLastName.Value));
                        }
                        else
                        {
                            fcm.notification.Add("body", String.Format("{0} {1} evden ayrılıyor. Borcunuz : {2:c}", userFirstName.Value,
                                                                                                            userLastName.Value,
                                                                                                            -friendship.Debt));
                        }
                    }
                    else
                    {
                        if (friendship.Debt > 0)
                        {
                            fcm.notification.Add("body", String.Format("{0} {1} evden ayrılıyor. Borcunuz : {2:c}", userFirstName.Value,
                                                                                                            userLastName.Value,
                                                                                                            friendship.Debt));
                        }
                        else if (friendship.Debt == 0)
                        {
                            fcm.notification.Add("body", String.Format("{0} {1} evden ayrılıyor. Borcunuz veya alacağınız bulunmamaktadır.", userFirstName.Value,
                                                                                                            userLastName.Value));
                        }
                        else
                        {
                            fcm.notification.Add("body", String.Format("{0} {1} evden ayrılıyor. Alacağınız : {2:c}", userFirstName.Value,
                                                                                                            userLastName.Value,
                                                                                                            -friendship.Debt));
                        }
                    }

                    await _fcmService.SendFCMAsync(fcm);
                    
                    fcm = new FCMModel(u.DeviceId, type : "LeaveHome");

                    fcm.data.Add("LeaverId", user.Id);
                    await _fcmService.SendFCMAsync(fcm);
                    
                    _friendshipRepository.Delete(friendship);
                }
            }
            else
            {
                ShoppingListModel shoppingList = await _shoppingListRepository.GetShoppingListByHomeIdAsync(user.Home.Id);
                List<NotepadModel> notepad = await _notepadRepository.GetAllNoteByHomeIdAsync(user.Home.Id);
                List<ExpenseModel> expenses = await _expenseRepository.GetAllExpensesByHomeIdAsync(user.Home.Id);

                _shoppingListRepository.Delete(shoppingList);

                foreach (var note in notepad)
                    _notepadRepository.Delete(note);

                foreach (var expense in expenses)
                    _expenseRepository.Delete(expense);

                user.Home = null;
                user.Position = (int)UserPosition.HasNotHome;

                _userRepository.Update(user);
                _homeRepository.Delete(home);
            }
        }

        public async Task BanishFromHomeAsync(UserModel user, int bannedUserId)
        {
            if (user.Position != (int)UserPosition.Admin)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Authorization Error", "User is not admin of home");
                errors.Throw();
            }

            UserModel bannedUser = await _userRepository.GetByIdAsync(bannedUserId, true);

            if(user.Id == bannedUser.Id)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Bad Request", "You can not bannish yourself");
                errors.Throw();
            }

            if (user.Home.Id == bannedUser.Home.Id)
                await LeaveHomeAsync(bannedUser, 0);
        }
    }
}
