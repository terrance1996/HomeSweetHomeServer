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
    public class UserExpenseService : IUserExpenseService
    {
        IExpenseRepository _expenseRepository;
        IUserExpenseRepository _userExpenseRepository;
        IFCMService _fcmService;
        IHomeService _homeService;
        IUserRepository _userRepository;
        IInformationRepository _informationRepository;
        IUserInformationRepository _userInformationRepository;
        IHomeRepository _homeRepository;

        public UserExpenseService(IExpenseRepository expenseRepository,
                                  IUserExpenseRepository userExpenseRepository,
                                  IFCMService fcmService,
                                  IHomeService homeService,
                                  IUserRepository userRepository,
                                  IInformationRepository informationRepository,
                                  IUserInformationRepository userInformationRepository,
                                  IHomeRepository homeRepository)
        {
            _expenseRepository = expenseRepository;
            _userExpenseRepository = userExpenseRepository;
            _fcmService = fcmService;
            _homeService = homeService;
            _userRepository = userRepository;
            _informationRepository = informationRepository;
            _userInformationRepository = userInformationRepository;
            _homeRepository = homeRepository;
        }

        //User adds expense
        public async Task AddExpenseAsync(UserModel user, ExpenseModel expense, List<int> participants)
        {
            if(user.Position == (int)UserPosition.HasNotHome)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Home Not Exist", "User is not member of a home");
                errors.Throw();
            }

            if ((expense.EType > (int)ExpenseType.Others) || (expense.EType < 0))
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Expense Type Not Exist", "Expense type number not valid");
                errors.Throw();
            }

            if (expense.Cost <= 0)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Expense Cost Not Valid", "The expense cost must be bigger than 0");
                errors.Throw();
            }

            participants = participants.Distinct().ToList();
            
            if (participants.Count == 0)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Participants Not Exist", "You must add participants for this expense");
                errors.Throw();
            }

            Task<InformationModel> firstNameInfo = _informationRepository.GetInformationByInformationNameAsync("FirstName");
            Task<InformationModel> lastNameInfo = _informationRepository.GetInformationByInformationNameAsync("LastName");

            user = await _userRepository.GetByIdAsync(user.Id, true);
            HomeModel home = await _homeRepository.GetByIdAsync(user.Home.Id, true);
            
            //Friend not found
            foreach (var p in participants)
            {
                if (home.Users.SingleOrDefault(u => u.Id == p) == null)
                {
                    CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                    errors.AddError("Friendship Not Found", "Friendship not found for lend");
                    errors.Throw();
                }
            }
            
            expense.LastUpdated = DateTime.UtcNow;
            expense.Home = home;
            expense.Author = await _userRepository.GetByIdAsync(user.Id); ;

            //Author informations
            UserInformationModel userFirstName = await _userInformationRepository.GetUserInformationByIdAsync(user.Id, (await firstNameInfo).Id);
            UserInformationModel userLastName = await _userInformationRepository.GetUserInformationByIdAsync(user.Id, (await lastNameInfo).Id);

            //Lend
            if (expense.EType == (int)ExpenseType.Lend)
            {
                
                if(participants.SingleOrDefault(p => p.Equals(user.Id)) != 0)
                {
                    CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                    errors.AddError("Lend Error", "You cant lend yourself");
                    errors.Throw();
                }

                ExpenseModel borrowExpense = new ExpenseModel((int)ExpenseType.Borrow,
                                                                  expense.Cost / participants.Count,
                                                                  expense.Author,
                                                                  expense.Home,
                                                                  expense.LastUpdated,
                                                                  expense.Title,
                                                                  expense.Content);
                
                _expenseRepository.Insert(expense);
                _expenseRepository.Insert(borrowExpense);

                //Borrow for all participants
                foreach (var p in participants)
                { 
                    var to = await _userRepository.GetByIdAsync(p);

                    await _homeService.TransferMoneyToFriendAsync(user, to, borrowExpense.Cost);
                    await _userExpenseRepository.InsertAsync(new UserExpenseModel(to, borrowExpense));
                    
                    //Send fcm to other participants
                    FCMModel fcmBorrow = new FCMModel(to.DeviceId, new Dictionary<string, object>());
                    fcmBorrow.notification.Add("title", "Nakit Aktarımı");
                    fcmBorrow.notification.Add("body", String.Format("{0} {1} tarafından {2:c} nakit alındı.",
                                                                                                    userFirstName.Value,
                                                                                                    userLastName.Value,
                                                                                                    borrowExpense.Cost));
                    await _fcmService.SendFCMAsync(fcmBorrow);

                    //Send fcm to other participants
                    fcmBorrow = new FCMModel(to.DeviceId, type : "AddExpense");
                    fcmBorrow.data.Add("Content", borrowExpense);
                    fcmBorrow.data.Add("Author", expense.Author.Username);
                    fcmBorrow.data.Add("Participants", participants);

                    await _fcmService.SendFCMAsync(fcmBorrow);
                }
                
                Task insertUEL = _userExpenseRepository.InsertAsync(new UserExpenseModel(user, expense));

                //Send fcm to user
                FCMModel fcmLend = new FCMModel(user.DeviceId, type : "AddExpense");
                fcmLend.data.Add("Content", expense);
                fcmLend.data.Add("Author", expense.Author.Username);
                fcmLend.data.Add("Participants", participants);

                await _fcmService.SendFCMAsync(fcmLend);

                await insertUEL;
            }

            else
            {
                expense.Cost = expense.Cost / participants.Count;
                _expenseRepository.Insert(expense);

                foreach (var p in participants)
                {
                    //Paid for friends
                    if (p != user.Id)
                    {
                        var to = await _userRepository.GetByIdAsync(p);

                        await _homeService.TransferMoneyToFriendAsync(user, to, expense.Cost);
                        await _userExpenseRepository.InsertAsync(new UserExpenseModel(to, expense));

                        //Send fcm to other participants
                        FCMModel fcmExpense = new FCMModel(to.DeviceId, new Dictionary<string, object>());
                        fcmExpense.notification.Add("title", String.Format("Yeni Gider : \"{0}\"", expense.Title));
                        fcmExpense.notification.Add("body", String.Format("{0} {1} tarafından {2:c} ödendi.", 
                                                                                                      userFirstName.Value,
                                                                                                      userLastName.Value,
                                                                                                      expense.Cost));

                        await _fcmService.SendFCMAsync(fcmExpense);

                        //Send fcm to other participants
                        fcmExpense = new FCMModel(to.DeviceId, type : "AddExpense");

                        fcmExpense.data.Add("Content", expense);
                        fcmExpense.data.Add("Author", expense.Author.Username);
                        fcmExpense.data.Add("Participants", participants);

                        await _fcmService.SendFCMAsync(fcmExpense);
                    }
                    else
                    {
                        Task insertUE = _userExpenseRepository.InsertAsync(new UserExpenseModel(user, expense));

                        //Send fcm to user
                        FCMModel fcmExpense = new FCMModel(user.DeviceId, type : "AddExpense");
                        fcmExpense.data.Add("Content", expense);
                        fcmExpense.data.Add("Author", expense.Author.Username);
                        fcmExpense.data.Add("Participants", participants);

                        await _fcmService.SendFCMAsync(fcmExpense);

                        await insertUE;
                    }
                }
                //Expense author is not exist in participants
                if (participants.SingleOrDefault(pr => pr == user.Id) == 0)
                {
                    //Send fcm to user
                    FCMModel fcmExpense = new FCMModel(user.DeviceId, type : "AddExpense");
                    fcmExpense.data.Add("Content", expense);
                    fcmExpense.data.Add("Author", expense.Author.Username);
                    fcmExpense.data.Add("Participants", participants);

                    await _fcmService.SendFCMAsync(fcmExpense);
                }
            }
        }

        //User deletes expense
        public async Task DeleteExpenseAsync(UserModel user, int expenseId)
        {
            if (user.Position == (int)UserPosition.HasNotHome)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Home Not Exist", "User is not member of a home");
                errors.Throw();
            }

            Task<InformationModel> firstNameInfo = _informationRepository.GetInformationByInformationNameAsync("FirstName");
            Task<InformationModel> lastNameInfo = _informationRepository.GetInformationByInformationNameAsync("LastName");

            user = await _userRepository.GetByIdAsync(user.Id, true);
            HomeModel home = await _homeRepository.GetByIdAsync(user.Home.Id, true);

            ExpenseModel expense = await _expenseRepository.GetExpenseByIdAsync(expenseId);

            if(expense == null)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Expense Not Found", "Expense not found, please check the expense id");
                errors.Throw();
            }

            if(!expense.Author.Equals(user))
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Authorization Error", "User is not author of this expense and can not delete");
                errors.Throw();
            }
            
            if (expense.EType == (int)ExpenseType.Borrow)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Authorization Error", "User can not delete a borrow expense, please contact with borrower");
                errors.Throw();
            }

            //Author informations
            UserInformationModel userFirstName = await _userInformationRepository.GetUserInformationByIdAsync(user.Id, (await firstNameInfo).Id);
            UserInformationModel userLastName = await _userInformationRepository.GetUserInformationByIdAsync(user.Id, (await lastNameInfo).Id);

            if (expense.EType == (int)ExpenseType.Lend)
            {
                ExpenseModel borrowExpense = await _expenseRepository.GetBorrowExpenseAfterLendExpenseAsync(user.Id, expense.Id);

                List<UserExpenseModel> participantsUE = await _userExpenseRepository.GetAllUserExpenseByExpenseIdAsync(borrowExpense.Id, true);

                foreach (var p in participantsUE)
                {
                    _userExpenseRepository.Delete(p);

                    await _homeService.TransferMoneyToFriendAsync(user, p.User, -borrowExpense.Cost);

                    //Send fcm to other participants
                    FCMModel fcmBorrow = new FCMModel(p.User.DeviceId, new Dictionary<string, object>());
                    fcmBorrow.notification.Add("title", String.Format("Nakit Aktarımı Iptal Edildi: \"{0}\"", borrowExpense.Title));
                    fcmBorrow.notification.Add("body", String.Format("{0} {1} tarafından verilen {2:c} geri alındı.",
                                                                                                  userFirstName.Value,
                                                                                                  userLastName.Value,
                                                                                                  borrowExpense.Cost));

                    await _fcmService.SendFCMAsync(fcmBorrow);

                    //Send fcm to other participants
                    fcmBorrow = new FCMModel(p.User.DeviceId, type : "DeleteExpense");

                    fcmBorrow.data.Add("ExpenseId", borrowExpense.Id);

                    await _fcmService.SendFCMAsync(fcmBorrow);
                }

                _expenseRepository.Delete(borrowExpense);

                //Send fcm to user
                FCMModel fcmLend = new FCMModel(user.DeviceId, type: "DeleteExpense");
                fcmLend.data.Add("ExpenseId", expense.Id);

                await _fcmService.SendFCMAsync(fcmLend);

                List<UserExpenseModel> lendUE = await _userExpenseRepository.GetAllUserExpenseByExpenseIdAsync(expense.Id, true);
                _userExpenseRepository.Delete(lendUE[0]);

                _expenseRepository.Delete(expense);
            }
            else
            {
                List<UserExpenseModel> participants = await _userExpenseRepository.GetAllUserExpenseByExpenseIdAsync(expense.Id, true);

                foreach (var p in participants)
                {
                    _userExpenseRepository.Delete(p);

                    //Refund to friends
                    if (p.User.Id != user.Id)
                    {
                        await _homeService.TransferMoneyToFriendAsync(user, p.User, -expense.Cost);

                        //Send fcm to other participants
                        FCMModel fcmExpense = new FCMModel(p.User.DeviceId, new Dictionary<string, object>());
                        fcmExpense.notification.Add("title", String.Format("Gider Iptal Edildi: \"{0}\"", expense.Title));
                        fcmExpense.notification.Add("body", String.Format("{0} {1} tarafından {2:c} iade edildi.",
                                                                                                      userFirstName.Value,
                                                                                                      userLastName.Value,
                                                                                                      expense.Cost));

                        await _fcmService.SendFCMAsync(fcmExpense);

                        //Send fcm to other participants
                        fcmExpense = new FCMModel(p.User.DeviceId, type : "DeleteExpense");
                        fcmExpense.data.Add("ExpenseId", expense.Id);

                        await _fcmService.SendFCMAsync(fcmExpense);
                    }
                    else
                    {
                        //Send fcm to user
                        FCMModel fcmExpense = new FCMModel(user.DeviceId, type : "DeleteExpense");
                        fcmExpense.data.Add("ExpenseId", expense.Id);

                        await _fcmService.SendFCMAsync(fcmExpense);
                    }
                }
                //Expense author is not exist in participants
                if (participants.SingleOrDefault(pr => pr.User.Id == user.Id) == null)
                {
                    //Send fcm to user
                    FCMModel fcmExpense = new FCMModel(user.DeviceId, type : "DeleteExpense");
                    fcmExpense.data.Add("ExpenseId", expense.Id);

                    await _fcmService.SendFCMAsync(fcmExpense);
                }

                _expenseRepository.Delete(expense);

            }
        }

        //User updates expense
        public async Task UpdateExpenseAsync(UserModel user, ExpenseModel expense, List<int> participants)
        {
            if (user.Position == (int)UserPosition.HasNotHome)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Home Not Exist", "User is not member of a home");
                errors.Throw();
            }

            if ((expense.EType > (int)ExpenseType.Others) || (expense.EType < 0))
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Expense Type Not Exist", "Expense type number not valid");
                errors.Throw();
            }

            if (expense.Cost <= 0)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Expense Cost Not Valid", "The expense cost must be bigger than 0");
                errors.Throw();
            }

            participants = participants.Distinct().ToList();

            if (participants.Count == 0)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Participants Not Exist", "You must add participants for this expense");
                errors.Throw();
            }

            Task<InformationModel> firstNameInfo = _informationRepository.GetInformationByInformationNameAsync("FirstName");
            Task<InformationModel> lastNameInfo = _informationRepository.GetInformationByInformationNameAsync("LastName");

            user = await _userRepository.GetByIdAsync(user.Id, true);
            HomeModel home = await _homeRepository.GetByIdAsync(user.Home.Id, true);

            //Friend not found
            foreach (var p in participants)
            {
                if (home.Users.SingleOrDefault(u => u.Id == p) == null)
                {
                    CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                    errors.AddError("Friendship Not Found", "Friendship not found for lend");
                    errors.Throw();
                }
            }
                        
            //Control old expense
            ExpenseModel oldExpense = await _expenseRepository.GetExpenseByIdAsync(expense.Id);

            if (oldExpense == null)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Expense Not Found", "Expense not found, please check the expense id");
                errors.Throw();
            }

            if (!oldExpense.Author.Equals(user))
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Authorization Error", "User is not author of this expense and can not delete");
                errors.Throw();
            }

            if (oldExpense.EType != expense.EType)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Expense Type Error", "Old and new expense types are not matched");
                errors.Throw();
            }

            //Author informations
            UserInformationModel userFirstName = await _userInformationRepository.GetUserInformationByIdAsync(user.Id, (await firstNameInfo).Id);
            UserInformationModel userLastName = await _userInformationRepository.GetUserInformationByIdAsync(user.Id, (await lastNameInfo).Id);
            
            expense.LastUpdated = DateTime.UtcNow;

            if (expense.EType == (int)ExpenseType.Lend)
            {
                if (participants.SingleOrDefault(p => p.Equals(user.Id)) != 0)
                {
                    CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                    errors.AddError("Lend Error", "You cant lend yourself");
                    errors.Throw();
                }

                ExpenseModel oldBorrowExpense = await _expenseRepository.GetBorrowExpenseAfterLendExpenseAsync(user.Id, oldExpense.Id);
                List<UserExpenseModel> oldParticipantsUE = await _userExpenseRepository.GetAllUserExpenseByExpenseIdAsync(oldBorrowExpense.Id, true);

                ExpenseModel newBorrowExpense = new ExpenseModel((int)ExpenseType.Borrow,
                                                                  expense.Cost / participants.Count,
                                                                  expense.Author,
                                                                  expense.Home,
                                                                  expense.LastUpdated,
                                                                  expense.Title,
                                                                  expense.Content);

                foreach (var p in oldParticipantsUE)
                {
                    //Old participant is not included in updated expense
                    if (participants.SingleOrDefault(pr => pr == p.User.Id) == 0)
                    {
                        _userExpenseRepository.Delete(p);
                        await _homeService.TransferMoneyToFriendAsync(user, p.User, -oldBorrowExpense.Cost);
                        
                        //Send fcm to participant
                        FCMModel fcmBorrow = new FCMModel(p.User.DeviceId, new Dictionary<string, object>());
                        fcmBorrow.notification.Add("title", String.Format("Gider Iptal Edildi: \"{0}\"", oldBorrowExpense.Title));
                        fcmBorrow.notification.Add("body", String.Format("{0} {1} tarafından verilen {2:c} geri alındı.",
                                                                                                      userFirstName.Value,
                                                                                                      userLastName.Value,
                                                                                                      oldBorrowExpense.Cost));


                        await _fcmService.SendFCMAsync(fcmBorrow);


                        //Send fcm to participant
                        fcmBorrow = new FCMModel(p.User.DeviceId, type : "DeleteExpense");

                        fcmBorrow.data.Add("ExpenseId", oldBorrowExpense.Id);
                        await _fcmService.SendFCMAsync(fcmBorrow);
                    }
                    //Old participant is included in updated expense
                    else
                    {
                        double difference = newBorrowExpense.Cost - oldBorrowExpense.Cost;
                        await _homeService.TransferMoneyToFriendAsync(user, p.User, difference);
                        
                        //Send fcm to other participants
                        FCMModel fcmBorrow = new FCMModel(p.User.DeviceId, new Dictionary<string, object>());
                        fcmBorrow.notification.Add("title", String.Format("Gider Güncellemesi : \"{0}\"", expense.Title));
                        fcmBorrow.notification.Add("body", String.Format("{0} {1} tarafından eklenen gider güncellendi.",
                                                                                                      userFirstName.Value,
                                                                                                      userLastName.Value));

                        await _fcmService.SendFCMAsync(fcmBorrow);

                        //Send fcm to other participants
                        fcmBorrow = new FCMModel(p.User.DeviceId, type : "UpdateExpense");

                        fcmBorrow.data.Add("Content", newBorrowExpense);
                        fcmBorrow.data.Add("Author", oldExpense.Author.Username);
                        fcmBorrow.data.Add("Participants", participants);

                        await _fcmService.SendFCMAsync(fcmBorrow);
                    }
                }

                oldBorrowExpense.Cost = newBorrowExpense.Cost;
                oldBorrowExpense.LastUpdated = newBorrowExpense.LastUpdated;
                oldBorrowExpense.Title = newBorrowExpense.Title;
                oldBorrowExpense.Content = newBorrowExpense.Content;               

                _expenseRepository.Update(oldBorrowExpense);

                oldExpense.Cost = expense.Cost;
                oldExpense.LastUpdated = expense.LastUpdated;
                oldExpense.Title = expense.Title;
                oldExpense.Content = expense.Content;

                _expenseRepository.Update(oldExpense);

                //Adds new participants to database
                List<UserExpenseModel> newParticipantsUE = await _userExpenseRepository.GetAllUserExpenseByExpenseIdAsync(oldBorrowExpense.Id, true);

                foreach(var p in participants)
                {
                    if(newParticipantsUE.SingleOrDefault(pr => pr.User.Id == p) == null)
                    {
                        var to = await _userRepository.GetByIdAsync(p);

                        await _homeService.TransferMoneyToFriendAsync(user, to, oldBorrowExpense.Cost);
                        await _userExpenseRepository.InsertAsync(new UserExpenseModel(to, oldBorrowExpense));

                        //Send fcm to other participants
                        FCMModel fcmBorrow = new FCMModel(to.DeviceId, new Dictionary<string, object>());
                        fcmBorrow.notification.Add("title", "Nakit Aktarımı");
                        fcmBorrow.notification.Add("body", String.Format("{0} {1} tarafından {2:c} nakit alındı.",
                                                                                                        userFirstName.Value,
                                                                                                        userLastName.Value,
                                                                                                        oldBorrowExpense.Cost));

                        await _fcmService.SendFCMAsync(fcmBorrow);

                        //Send fcm to other participants
                        fcmBorrow = new FCMModel(to.DeviceId, type : "AddExpense");

                        fcmBorrow.data.Add("Content", oldBorrowExpense);
                        fcmBorrow.data.Add("Author", oldBorrowExpense.Author.Username);
                        fcmBorrow.data.Add("Participants", participants);

                        await _fcmService.SendFCMAsync(fcmBorrow);
                    }

                    //Send fcm to other participants
                    FCMModel fcmLend = new FCMModel(user.DeviceId, type: "UpdateExpense");

                    fcmLend.data.Add("Content", oldBorrowExpense);
                    fcmLend.data.Add("Author", oldExpense.Author.Username);
                    fcmLend.data.Add("Participants", participants);

                    await _fcmService.SendFCMAsync(fcmLend);
                }
            }
            else
            {
                List<UserExpenseModel> oldParticipants = await _userExpenseRepository.GetAllUserExpenseByExpenseIdAsync(expense.Id, true);
                
                expense.Cost = expense.Cost / participants.Count;
                
                foreach (var p in oldParticipants)
                {
                    //Old participant is not included in updated expense
                    if (participants.SingleOrDefault(pr => pr == p.User.Id) == 0)
                    {
                        _userExpenseRepository.Delete(p);

                        //Other participants
                        if (p.User.Id != user.Id)
                        {
                            await _homeService.TransferMoneyToFriendAsync(user, p.User, -oldExpense.Cost);

                            //Send fcm to participant
                            FCMModel fcmExpense = new FCMModel(p.User.DeviceId, new Dictionary<string, object>());
                            fcmExpense.notification.Add("title", String.Format("Gider Iptal Edildi: \"{0}\"", oldExpense.Title));
                            fcmExpense.notification.Add("body", String.Format("{0} {1} tarafından {2:c} iade edildi.",
                                                                                                      userFirstName.Value,
                                                                                                      userLastName.Value,
                                                                                                      oldExpense.Cost));

                            await _fcmService.SendFCMAsync(fcmExpense);

                            //Send fcm to participant
                            fcmExpense = new FCMModel(p.User.DeviceId, type : "DeleteExpense");

                            fcmExpense.data.Add("ExpenseId", oldExpense.Id);
                            await _fcmService.SendFCMAsync(fcmExpense);
                        }
                    }
                    //Old participant is included in updated expense
                    else
                    {
                        if (p.User.Id != user.Id)
                        {

                            double difference = expense.Cost - oldExpense.Cost;
                            await _homeService.TransferMoneyToFriendAsync(user, p.User, difference);

                            //Send fcm to other participants
                            FCMModel fcmExpense = new FCMModel(p.User.DeviceId, new Dictionary<string, object>());
                            fcmExpense.notification.Add("title", String.Format("Gider Güncellemesi : \"{0}\"", expense.Title));
                            fcmExpense.notification.Add("body", String.Format("{0} {1} tarafından eklenen gider güncellendi.",
                                                                                                          userFirstName.Value,
                                                                                                          userLastName.Value));

                            await _fcmService.SendFCMAsync(fcmExpense);

                            //Send fcm to other participants
                            fcmExpense = new FCMModel(p.User.DeviceId, type : "UpdateExpense");

                            fcmExpense.data.Add("Content", expense);
                            fcmExpense.data.Add("Author", oldExpense.Author.Username);
                            fcmExpense.data.Add("Participants", participants);

                            await _fcmService.SendFCMAsync(fcmExpense);
                        }
                        else
                        {
                            FCMModel fcmExpense = new FCMModel(p.User.DeviceId, new Dictionary<string, object>(), "UpdateExpense");
                            fcmExpense.data.Add("Content", expense);
                            fcmExpense.data.Add("Author", oldExpense.Author.Username);
                            fcmExpense.data.Add("Participants", participants);

                            await _fcmService.SendFCMAsync(fcmExpense);
                        }
                    }
                }

                oldExpense.Cost = expense.Cost;
                oldExpense.LastUpdated = expense.LastUpdated;
                oldExpense.Title = expense.Title;
                oldExpense.Content = expense.Content;

                _expenseRepository.Update(oldExpense);

                //Adds new participants to database
                List<UserExpenseModel> newParticipantsUE = await _userExpenseRepository.GetAllUserExpenseByExpenseIdAsync(oldExpense.Id, true);

                foreach (var p in participants)
                {
                    if (newParticipantsUE.SingleOrDefault(pr => pr.User.Id == p) == null)
                    {
                        if (p != user.Id)
                        {
                            var to = await _userRepository.GetByIdAsync(p);

                            await _homeService.TransferMoneyToFriendAsync(user, to, oldExpense.Cost);
                            await _userExpenseRepository.InsertAsync(new UserExpenseModel(to, oldExpense));

                            //Send fcm to other participants
                            FCMModel fcmExpense = new FCMModel(to.DeviceId, new Dictionary<string, object>());
                            fcmExpense.notification.Add("title", String.Format("Yeni Gider : \"{0}\"", oldExpense.Title));
                            fcmExpense.notification.Add("body", String.Format("{0} {1} tarafından {2:c} ödendi.",
                                                                                                          userFirstName.Value,
                                                                                                          userLastName.Value,
                                                                                                          oldExpense.Cost));

                            await _fcmService.SendFCMAsync(fcmExpense);

                            //Send fcm to other participants
                            fcmExpense = new FCMModel(to.DeviceId, type : "AddExpense");

                            fcmExpense.data.Add("Content", oldExpense);
                            fcmExpense.data.Add("Author", oldExpense.Author.Username);
                            fcmExpense.data.Add("Participants", participants);

                            await _fcmService.SendFCMAsync(fcmExpense);
                        }
                        else
                        {
                            Task insertUE = _userExpenseRepository.InsertAsync(new UserExpenseModel(user, oldExpense));

                            //Send fcm to user
                            FCMModel fcmExpense = new FCMModel(user.DeviceId, new Dictionary<string, object>(), "UpdateExpense");
                            fcmExpense.data.Add("Content", oldExpense);
                            fcmExpense.data.Add("Author", oldExpense.Author.Username);
                            fcmExpense.data.Add("Participants", participants);

                            await _fcmService.SendFCMAsync(fcmExpense);

                            await insertUE;
                        }
                    }
                }
                //Expense author is not exist in participants
                if(participants.SingleOrDefault(pr => pr.Equals(user.Id)) == 0)
                {
                    FCMModel fcmExpense = new FCMModel(user.DeviceId, new Dictionary<string, object>(), "UpdateExpense");
                    fcmExpense.data.Add("Content", expense);
                    fcmExpense.data.Add("Author", oldExpense.Author.Username);
                    fcmExpense.data.Add("Participants", participants);

                    await _fcmService.SendFCMAsync(fcmExpense);
                }
            }
        }
    }
}
