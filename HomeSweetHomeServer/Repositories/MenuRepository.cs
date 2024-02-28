using System.Collections.Generic;
using System.Threading.Tasks;
using HomeSweetHomeServer.Models;
using HomeSweetHomeServer.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;

namespace HomeSweetHomeServer.Repositories
{
    public class MenuRepository : BaseRepository<MenuModel>, IMenuRepository
    {
        public MenuRepository(DatabaseContext context) : base(context)
        {
        }

        //Gets home menu by date
        public async Task<MenuModel> GetHomeMenuByDateAsync(int homeId, DateTime date, bool include = false)
        {
            if (include == false)
                return await Db.SingleOrDefaultAsync(m => m.Home.Id == homeId && m.Date == date);
            else
                return await Db.Include(m => m.Home).SingleOrDefaultAsync(m => m.Home.Id == homeId && m.Date == date);
        }
        
        //Gets all home menus
        public async Task<List<MenuModel>> GetAllHomeMenusAsync(int homeId, bool include = false)
        {
            if (include == false)
                return await Db.Where(m => m.Home.Id == homeId).ToListAsync();
            else
                return await Db.Include(m => m.Home).Where(m => m.Home.Id == homeId).ToListAsync();
        }

        //Gets menu by menu id
        public async Task<MenuModel> GetHomeMenuByIdAsync(int menuId, bool include = false)
        {
            if (include == false)
                return await Db.SingleOrDefaultAsync(m => m.Id == menuId);
            else
                return await Db.Include(m => m.Home).SingleOrDefaultAsync(m => m.Id == menuId);
        }

    }
}
