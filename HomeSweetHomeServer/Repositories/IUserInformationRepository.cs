using System.Collections.Generic;
using System.Threading.Tasks;
using HomeSweetHomeServer.Models;

namespace HomeSweetHomeServer.Repositories
{
    //Interface about user information repository operations
    public interface IUserInformationRepository : IBaseRepository<UserInformationModel>
    {
        Task<UserInformationModel> GetUserInformationByValueAsync(string value, bool include = false);
        Task<List<UserInformationModel>> GetAllUserInformationsByUserIdAsync(int userId, bool include = false);
        Task<UserInformationModel> GetUserInformationByIdAsync(int userId, int informationId, bool include = false);
    }
}
