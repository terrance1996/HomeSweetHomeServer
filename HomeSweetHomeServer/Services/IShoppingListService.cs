using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeSweetHomeServer.Models;

namespace HomeSweetHomeServer.Services
{
    //Interface about shopping list operations
    public interface IShoppingListService
    {
        Task<ShoppingListModel> SynchronizeShoppingListAsync(UserModel user);
        Task UpdateShoppingListAsync(UserModel user, ShoppingListModel shoppingList);
        Task SendNotification(UserModel user);
    }
}
