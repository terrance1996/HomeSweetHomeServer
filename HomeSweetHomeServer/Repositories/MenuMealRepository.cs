using System.Collections.Generic;
using System.Threading.Tasks;
using HomeSweetHomeServer.Models;
using HomeSweetHomeServer.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HomeSweetHomeServer.Repositories
{
    public class MenuMealRepository : BaseRepository<MenuMealModel>, IMenuMealRepository
    {
        public MenuMealRepository(DatabaseContext context) : base(context)
        {
        }

        //Gets all menu meals by menu id
        public async Task<List<MenuMealModel>> GetAllMenuMealsByMenuIdAsync(int menuId, bool include = false)
        {
            if (include == false)
                return await Db.Where(m => m.Menu.Id == menuId).ToListAsync();
            else
                return await Db.Include(m => m.Menu).Include(m => m.Meal).Where(m => m.Menu.Id == menuId).ToListAsync();
        }
        
        //Gets all menu meals by meal id
        public async Task<List<MenuMealModel>> GetAllMenuMealsByMealIdAsync(int mealId, bool include = false)
        {
            if (include == false)
                return await Db.Where(m => m.Meal.Id == mealId).ToListAsync();
            else
                return await Db.Include(m => m.Menu).Include(m => m.Meal).Where(m => m.Meal.Id == mealId).ToListAsync();
        }

    }
}
