using System.Collections.Generic;
using System.Threading.Tasks;
using HomeSweetHomeServer.Models;
using HomeSweetHomeServer.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HomeSweetHomeServer.Repositories
{
    public class ShoppingListRepository : BaseRepository<ShoppingListModel>, IShoppingListRepository
    {
        public ShoppingListRepository(DatabaseContext context) : base(context)
        {
        }
        
        //Gets home shopping list by shopping list id
        public async Task<ShoppingListModel> GetShoppingListByHomeIdAsync(int homeId, bool include = false)
        {
            if (include == false)
                return await Db.SingleOrDefaultAsync(s => s.Home.Id == homeId);
            else
                return await Db.Include(s => s.Home).ThenInclude(h => h.Users).SingleOrDefaultAsync(s => s.Home.Id == homeId);
        }
    }
}
