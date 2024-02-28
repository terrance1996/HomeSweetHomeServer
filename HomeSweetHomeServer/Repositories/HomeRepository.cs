using System.Collections.Generic;
using System.Threading.Tasks;
using HomeSweetHomeServer.Models;
using HomeSweetHomeServer.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HomeSweetHomeServer.Repositories
{
    public class HomeRepository : BaseRepository<HomeModel>, IHomeRepository
    {
        public HomeRepository(DatabaseContext context) : base(context)
        {
            
        }

        //Gets home by home id
        public async Task<HomeModel> GetByIdAsync(int id, bool include = false)
        {
            if (include == false)
                return await Db.SingleOrDefaultAsync(h => h.Id == id);
            else
                return await Db.Include(h => h.Admin).Include(h => h.Users).SingleOrDefaultAsync(h => h.Id == id);
        }

        //Gets home by home name
        public async Task<HomeModel> GetByHomeNameAsync(string name, bool include = false)
        {
            if (include == false)
                return await Db.SingleOrDefaultAsync(h => h.Name == name);
            else
                return await Db.Include(h => h.Admin).Include(h => h.Users).SingleOrDefaultAsync(u => u.Name == name);
        }
    }
}
