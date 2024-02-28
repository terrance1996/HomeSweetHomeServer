using System.Collections.Generic;
using System.Threading.Tasks;
using HomeSweetHomeServer.Models;
using HomeSweetHomeServer.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HomeSweetHomeServer.Repositories
{
    public class HouseworkRepository : BaseRepository<HouseworkModel>, IHouseworkRepository
    {
        public HouseworkRepository(DatabaseContext context) : base(context)
        {
        }

        //Gets all home houseworks
        public async Task<List<HouseworkModel>> GetAllHomeHouseworksAsync(int homeId, bool include = false)
        {
            if (include == false)
                return await Db.Where(hw => hw.Home.Id == homeId).ToListAsync();
            else
                return await Db.Include(hw => hw.User).Include(hw => hw.Home).Where(hw => hw.Home.Id == homeId).ToListAsync();
        }

        //Gets all user houseworks
        public async Task<List<HouseworkModel>> GetAllUserHouseworksAsync(int userId, bool include = false)
        {
            if (include == false)
                return await Db.Where(hw => hw.User.Id == userId).ToListAsync();
            else
                return await Db.Include(hw => hw.User).Include(hw => hw.Home).Where(hw => hw.User.Id == userId).ToListAsync();
        }

        //Gets housework by housework id
        public async Task<HouseworkModel> GetHouseworkByIdAsync(int id, bool include = false)
        {
            if (include == false)
                return await Db.SingleOrDefaultAsync(hw => hw.Id == id);
            else
                return await Db.Include(hw => hw.User).Include(hw => hw.Home).SingleOrDefaultAsync(hw => hw.Id == id);
        }
    }
}
