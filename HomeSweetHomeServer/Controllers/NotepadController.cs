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
    [Route("api/Notepad")]
    [Authorize]
    public class NotepadController : Controller
    {
        IJwtTokenService _jwtTokenService;
        INotepadService _notepadService;

        public NotepadController(IJwtTokenService jwtTokenService, INotepadService notepadService)
        {
            _jwtTokenService = jwtTokenService;
            _notepadService = notepadService;
        }

        //Synchronizes clients notepad
        [HttpGet("Synchronize", Name = "SynchronizeNotepad")]
        public async Task<IActionResult> Synchronize()
        {
            string token = Request.Headers["Authorization"].ToString().Substring("Bearer ".Length).Trim();
            UserModel user = await _jwtTokenService.GetUserFromTokenStrAsync(token);

            List<NotepadModel> res = await _notepadService.SynchronizeNotepadAsync(user);
            
            return Ok(res);
        }

        //User adds note
        [HttpPost("AddNote", Name = "AddNote")]
        public async Task<IActionResult> AddNote([FromBody] NotepadModel note)
        {
            string token = Request.Headers["Authorization"].ToString().Substring("Bearer ".Length).Trim();
            UserModel user = await _jwtTokenService.GetUserFromTokenStrAsync(token);
            
            await _notepadService.AddNoteAsync(user, note);

            return Ok();
        }

        //User deletes note
        [HttpGet("DeleteNote", Name = "DeleteNote")]
        public async Task<IActionResult> DeleteNote([FromQuery] int noteId)
        {
            string token = Request.Headers["Authorization"].ToString().Substring("Bearer ".Length).Trim();
            UserModel user = await _jwtTokenService.GetUserFromTokenStrAsync(token);

            await _notepadService.DeleteNoteAsync(user, noteId);

            return Ok();
        }

        //User updates note
        [HttpPost("UpdateNote", Name = "UpdateNote")]
        public async Task<IActionResult> UpdateNote([FromBody] NotepadModel note)
        {
            string token = Request.Headers["Authorization"].ToString().Substring("Bearer ".Length).Trim();
            UserModel user = await _jwtTokenService.GetUserFromTokenStrAsync(token);

            await _notepadService.UpdateNoteAsync(user, note);

            return Ok();
        }
    }
}
