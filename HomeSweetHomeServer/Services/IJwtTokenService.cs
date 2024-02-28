using HomeSweetHomeServer.Models;
using System.Threading.Tasks;

namespace HomeSweetHomeServer.Services
{
    //Interface about token operations
    public interface IJwtTokenService
    {
        string CreateToken(UserModel user);
        bool VerifyToken(string token);
        Task<UserModel> GetUserFromTokenStrAsync(string tokenstr);
    }
}
