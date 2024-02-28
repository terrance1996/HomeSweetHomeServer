using System.Collections.Generic;
using System.Threading.Tasks;
using HomeSweetHomeServer.Models;

namespace HomeSweetHomeServer.Repositories
{
    //Interface about meal repository operations
    public interface IMealRepository : IBaseRepository<MealModel>
    {
        Task<MealModel> GetHomeMealByNameAsync(int homeId, string name, bool include = false);
        Task<MealModel> GetHomeMealByIdAsync(int mealId, bool include = false);
        Task<List<MealModel>> GetAllHomeMealsAsync(int homeId, bool include = false);
    }
}
