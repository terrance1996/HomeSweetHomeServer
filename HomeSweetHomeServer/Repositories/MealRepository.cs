using System.Collections.Generic;
using System.Threading.Tasks;
using HomeSweetHomeServer.Models;
using HomeSweetHomeServer.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HomeSweetHomeServer.Repositories
{
    public class MealRepository : BaseRepository<MealModel>, IMealRepository
    {
        public MealRepository(DatabaseContext context) : base(context)
        {
        }

        //Gets home meal by name
        public async Task<MealModel> GetHomeMealByNameAsync(int homeId, string name, bool include = false)
        {
            if (include == false)
                return await Db.SingleOrDefaultAsync(m => m.Home.Id == homeId && m.Name == name);
            else
                return await Db.Include(m => m.Home).SingleOrDefaultAsync(m => m.Home.Id == homeId && m.Name == name);
        }

        //Gets home meal by id
        public async Task<MealModel> GetHomeMealByIdAsync(int mealId, bool include = false)
        {
            if (include == false)
                return await Db.SingleOrDefaultAsync(m => m.Id == mealId);
            else
                return await Db.Include(m => m.Home).SingleOrDefaultAsync(m => m.Id == mealId);
        }

        //Gets all home meals
        public async Task<List<MealModel>> GetAllHomeMealsAsync(int homeId, bool include = false)
        {
            if (include == false)
                return await Db.Where(m => m.Home.Id == homeId).ToListAsync();
            else
                return await Db.Include(m => m.Home).Where(m => m.Home.Id == homeId).ToListAsync();
        }
    }
}
