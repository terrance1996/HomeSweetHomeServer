using System.Collections.Generic;
using System.Threading.Tasks;
using HomeSweetHomeServer.Models;

namespace HomeSweetHomeServer.Repositories
{
    //Interface about expense repository operations
    public interface IHouseworkRepository : IBaseRepository<HouseworkModel>
    {
        Task<List<HouseworkModel>> GetAllHomeHouseworksAsync(int homeId, bool include = false);
        Task<List<HouseworkModel>> GetAllUserHouseworksAsync(int userId, bool include = false);
        Task<HouseworkModel> GetHouseworkByIdAsync(int id, bool include = false);
    }
}
