using System.Collections.Generic;
using System.Threading.Tasks;
using HomeSweetHomeServer.Models;
using HomeSweetHomeServer.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HomeSweetHomeServer.Repositories
{
    public class ExpenseRepository : BaseRepository<ExpenseModel>, IExpenseRepository
    {
        public ExpenseRepository(DatabaseContext context) : base(context)
        {
        }

        //Gets expense by expense id
        public async Task<ExpenseModel> GetExpenseByIdAsync(int id, bool include = false)
        {
            if (include == false)
                return await Db.SingleOrDefaultAsync(e => e.Id == id);
            else
                return await Db.Include(e => e.Author).Include(e => e.Home).SingleOrDefaultAsync(e => e.Id == id);
        }

        //Gets all home expenses
        public async Task<List<ExpenseModel>> GetAllExpensesByHomeIdAsync(int homeId, bool include = false)
        {
            if (include == false)
                return await Db.Where(e => e.Home.Id == homeId).ToListAsync();
            else
                return await Db.Include(e => e.Author).Include(e => e.Home).Where(e => e.Home.Id == homeId).ToListAsync();
        }

        //Gets user the first borrow expense after lend expense
        public async Task<ExpenseModel> GetBorrowExpenseAfterLendExpenseAsync(int authorId, int lendExpenseId, bool include = false)
        {
            if (include == false)
                return await Db.FirstAsync(e => e.Author.Id == authorId && e.Id > lendExpenseId);
            else
                return await Db.Include(e => e.Author).Include(e => e.Home).FirstAsync(e => e.Author.Id == authorId && e.Id > lendExpenseId);
        }
    }
}
