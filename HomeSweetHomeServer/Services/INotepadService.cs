using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeSweetHomeServer.Models;

namespace HomeSweetHomeServer.Services
{
    //Interface about notepad operations
    public interface INotepadService
    {
        Task<List<NotepadModel>> SynchronizeNotepadAsync(UserModel user);
        Task AddNoteAsync(UserModel user, NotepadModel note);
        Task DeleteNoteAsync(UserModel user, int noteId);
        Task UpdateNoteAsync(UserModel user, NotepadModel note);
    }
}
