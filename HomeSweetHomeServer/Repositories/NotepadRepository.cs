using System.Collections.Generic;
using System.Threading.Tasks;
using HomeSweetHomeServer.Models;
using HomeSweetHomeServer.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HomeSweetHomeServer.Repositories
{
    public class NotepadRepository : BaseRepository<NotepadModel>, INotepadRepository
    {
        public NotepadRepository(DatabaseContext context) : base(context)
        {
        }

        //Gets all note of home by home id
        public async Task<List<NotepadModel>> GetAllNoteByHomeIdAsync(int homeId, bool include = false)
        {
            if (include == false)
                return await Db.Where(n => n.Home.Id == homeId).ToListAsync();
            else
                return await Db.Where(n => n.Home.Id == homeId).Include(n => n.Home).ToListAsync(); 
        }

        //Gets note by note id
        public async Task<NotepadModel> GetNoteByIdAsync(int id, bool include = false)
        {
            if (include == false)
                return await Db.SingleOrDefaultAsync(n => n.Id == id);
            else
                return await Db.Include(n => n.Home).SingleOrDefaultAsync(n => n.Id == id);
        }

    }
}
