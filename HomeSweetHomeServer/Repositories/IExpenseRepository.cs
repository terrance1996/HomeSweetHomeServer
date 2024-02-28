using System.Collections.Generic;
using System.Threading.Tasks;
using HomeSweetHomeServer.Models;

namespace HomeSweetHomeServer.Repositories
{
    //Interface about expense repository operations
    public interface IExpenseRepository : IBaseRepository<ExpenseModel>
    {
        Task<ExpenseModel> GetExpenseByIdAsync(int id, bool include = false);
        Task<List<ExpenseModel>> GetAllExpensesByHomeIdAsync(int homeId, bool include = false);
        Task<ExpenseModel> GetBorrowExpenseAfterLendExpenseAsync(int authorId, int lendExpenseId, bool include = false);
    }
}
