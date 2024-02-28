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
    public class NotepadService : INotepadService
    {
        IInformationRepository _informationRepository;
        IUserRepository _userRepository;
        IUserInformationRepository _userInformationRepository;
        IConfiguration _config;
        IMailService _mailService;
        IHomeRepository _homeRepository;
        IFCMService _fcmService;
        IFriendshipRepository _friendshipRepository;
        INotepadRepository _notepadRepository;

        public NotepadService(IInformationRepository informationRepository,
                           IUserRepository userRepository,
                           IUserInformationRepository userInformationRepository,
                           IConfiguration config,
                           IMailService mailService,
                           IHomeRepository homeRepository,
                           IFCMService fcmService,
                           IFriendshipRepository friendshipRepository,
                           INotepadRepository notepadRepository)
        {
            _informationRepository = informationRepository;
            _userRepository = userRepository;
            _userInformationRepository = userInformationRepository;
            _config = config;
            _mailService = mailService;
            _homeRepository = homeRepository;
            _fcmService = fcmService;
            _friendshipRepository = friendshipRepository;
            _notepadRepository = notepadRepository;
        }

        //Synchronizes clients notepad
        public async Task<List<NotepadModel>> SynchronizeNotepadAsync(UserModel user)
        {
            if(user.Position == (int)UserPosition.HasNotHome)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Home Not Exist", "User is not member of a home");
                errors.Throw();
            }

            user = await _userRepository.GetByIdAsync(user.Id, true);

            return await _notepadRepository.GetAllNoteByHomeIdAsync(user.Home.Id);
        }

        //User adds note
        public async Task AddNoteAsync(UserModel user, NotepadModel note)
        {
            if (user.Position == (int)UserPosition.HasNotHome)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Home Not Exist", "User is not member of a home");
                errors.Throw();
            }

            user = await _userRepository.GetByIdAsync(user.Id, true);
            HomeModel home = await _homeRepository.GetByIdAsync(user.Home.Id, true);
            note.Home = home;

            await _notepadRepository.InsertAsync(note);

            foreach (var friend in home.Users)
            {
                FCMModel fcm = new FCMModel(friend.DeviceId, type: "NotepadAdd");
                fcm.data.Add("NewNote", note);
                await _fcmService.SendFCMAsync(fcm);
            }

        }

        //User deletes note
        public async Task DeleteNoteAsync(UserModel user, int noteId)
        {
            if (user.Position == (int)UserPosition.HasNotHome)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Home Not Exist", "User is not member of a home");
                errors.Throw();
            }

            user = await _userRepository.GetByIdAsync(user.Id, true);
            HomeModel home = await _homeRepository.GetByIdAsync(user.Home.Id, true);

            NotepadModel note = await _notepadRepository.GetNoteByIdAsync(noteId, true);

            if(note == null)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Note Not Exist", "Note is not exist");
                errors.Throw();
            }

            if(note.Home != home)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Note Not Belongs Home", "Note does not belong this home");
                errors.Throw();
            }

            foreach (var friend in home.Users)
            {
                FCMModel fcm = new FCMModel(friend.DeviceId, type: "NotepadDelete");
                fcm.data.Add("DeletedNote", note.Id);
                await _fcmService.SendFCMAsync(fcm);
            }

            await _notepadRepository.DeleteAsync(note);
        }

        //User updates note
        public async Task UpdateNoteAsync(UserModel user, NotepadModel note)
        {
            if (user.Position == (int)UserPosition.HasNotHome)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Home Not Exist", "User is not member of a home");
                errors.Throw();
            }

            if(note.Id == 0)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Id Not Exist", "Notepad id field is required");
                errors.Throw();
            }

            user = await _userRepository.GetByIdAsync(user.Id, true);
            HomeModel home = await _homeRepository.GetByIdAsync(user.Home.Id, true);
            NotepadModel old = await _notepadRepository.GetNoteByIdAsync(note.Id, true);

            if(old == null)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Note Not Exist", "Note is not exist");
                errors.Throw();
            }

            if (old.Id != note.Id)
            {
                CustomException errors = new CustomException((int)HttpStatusCode.BadRequest);
                errors.AddError("Note Not Belongs Home", "Note does not belong this home");
                errors.Throw();
            }

            old.Title = note.Title;
            old.Content = note.Content;

            await _notepadRepository.UpdateAsync(old);
            
            foreach (var friend in home.Users)
            {
                FCMModel fcm = new FCMModel(friend.DeviceId, type: "NotepadUpdate");
                fcm.data.Add("UpdatedNote", old);
                await _fcmService.SendFCMAsync(fcm);
            }
        }
    }
}
