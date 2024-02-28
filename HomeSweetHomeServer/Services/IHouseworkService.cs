using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeSweetHomeServer.Models;

namespace HomeSweetHomeServer.Services
{
    //Interface about housework operations
    public interface IHouseworkService
    {
        Task<List<ClientHouseworkModel>> SynchronizeHouseworksAsync(UserModel user);
        Task AddHouseworkAsync(UserModel user, HouseworkModel housework, int friendId);
        Task DeleteHouseworkAsync(UserModel user, int houseworkId);
        Task UpdateHouseworkAsync(UserModel user, HouseworkModel housework, int friendId);
    }
}
