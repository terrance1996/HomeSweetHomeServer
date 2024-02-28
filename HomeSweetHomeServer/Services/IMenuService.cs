using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeSweetHomeServer.Models;

namespace HomeSweetHomeServer.Services
{
    //Interface about menu operations
    public interface IMenuService
    {
        Task<List<ClientMenuModel>> SynchronizeMenusAsync(UserModel user);
        Task AddMenuAsync(UserModel user, MenuModel menu, List<int> mealIds);
        Task UpdateMenuAsync(UserModel user, MenuModel menu, List<int> mealIds);
        Task DeleteMenuAsync(UserModel user, int menuId);
        Task<List<MealModel>> SynchronizeMealsAsync(UserModel user);
        Task AddMealAsync(UserModel user, MealModel meal);
        Task UpdateMealAsync(UserModel user, MealModel meal);
        Task DeleteMealAsync(UserModel user, int mealId);
    }
}
