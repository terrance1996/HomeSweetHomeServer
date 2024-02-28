using System.Collections.Generic;
using System.Threading.Tasks;
using HomeSweetHomeServer.Models;

namespace HomeSweetHomeServer.Repositories
{
    //Interface about menu meal repository operations
    public interface IMenuMealRepository : IBaseRepository<MenuMealModel>
    {
        Task<List<MenuMealModel>> GetAllMenuMealsByMenuIdAsync(int menuId, bool include = false);
        Task<List<MenuMealModel>> GetAllMenuMealsByMealIdAsync(int mealId, bool include = false);
    }
}
