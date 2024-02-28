using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeSweetHomeServer.Models;

namespace HomeSweetHomeServer.Repositories
{
    //Interface about home repository operations
    public interface IHomeRepository : IBaseRepository<HomeModel>
    {
        Task<HomeModel> GetByIdAsync(int id, bool include = false);
        Task<HomeModel> GetByHomeNameAsync(string name, bool include = false);
    }
}
