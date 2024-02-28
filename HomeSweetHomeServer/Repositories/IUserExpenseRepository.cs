using System.Collections.Generic;
using System.Threading.Tasks;
using HomeSweetHomeServer.Models;

namespace HomeSweetHomeServer.Repositories
{
    //Interface about user expense repository operations
    public interface IUserExpenseRepository : IBaseRepository<UserExpenseModel>
    {
        Task<List<UserExpenseModel>> GetAllUserExpenseByUserIdAsync(int userId, bool include = false);
        Task<List<UserExpenseModel>> GetAllUserExpenseByExpenseIdAsync(int expenseId, bool include = false);
        Task<UserExpenseModel> GetUserExpenseByIdAsync(int id, bool include = false);
    }
}
