using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HomeSweetHomeServer.Models;

namespace HomeSweetHomeServer.Repositories
{
    //Interface about menu repository operations
    public interface IMenuRepository : IBaseRepository<MenuModel>
    {
        Task<MenuModel> GetHomeMenuByDateAsync(int homeId, DateTime date, bool include = false);
        Task<List<MenuModel>> GetAllHomeMenusAsync(int homeId, bool include = false);
        Task<MenuModel> GetHomeMenuByIdAsync(int menuId, bool include = false);
    }
}
