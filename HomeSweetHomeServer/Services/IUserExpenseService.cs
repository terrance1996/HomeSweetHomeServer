using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeSweetHomeServer.Models;

namespace HomeSweetHomeServer.Services
{
    //Interface about user expense operations
    public interface IUserExpenseService
    {
        Task AddExpenseAsync(UserModel user, ExpenseModel expense, List<int> participants);
        Task DeleteExpenseAsync(UserModel user, int expenseId);
        Task UpdateExpenseAsync(UserModel user, ExpenseModel expense, List<int> participants);
    }
}
