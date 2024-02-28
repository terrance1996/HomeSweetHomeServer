using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeSweetHomeServer.Models;

namespace HomeSweetHomeServer.Repositories
{
    //Interface about notepad repository operations
    public interface INotepadRepository : IBaseRepository<NotepadModel>
    {
        Task<List<NotepadModel>> GetAllNoteByHomeIdAsync(int homeId, bool include = false);
        Task<NotepadModel> GetNoteByIdAsync(int id, bool include = false);
    }
}
