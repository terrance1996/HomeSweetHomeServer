using System.Collections.Generic;
using System.Threading.Tasks;
using HomeSweetHomeServer.Models;
using HomeSweetHomeServer.Contexts;
using Microsoft.EntityFrameworkCore;

namespace HomeSweetHomeServer.Repositories
{
    public class FriendshipRepository : BaseRepository<FriendshipModel>, IFriendshipRepository
    {
        public FriendshipRepository(DatabaseContext context) : base(context)
        {
        }

        //Gets friendship by user1 and user2 id
        public async Task<FriendshipModel> GetFriendshipByIdAsync(int user1Id, int user2Id, bool include = false)
        {
            if(include == false)
            {
                var friendship = await Db.SingleOrDefaultAsync(f => f.User1.Id == user1Id && f.User2.Id == user2Id);

                if(friendship == null)
                    friendship = await Db.SingleOrDefaultAsync(f => f.User1.Id == user2Id && f.User2.Id == user1Id);

                return friendship;
            }
            else
            {
                var friendship = await Db.Include(f => f.User1).Include(f => f.User2).SingleOrDefaultAsync(f => f.User1.Id == user1Id && f.User2.Id == user2Id);

                if (friendship == null)
                    friendship = await Db.Include(f => f.User1).Include(f => f.User2).SingleOrDefaultAsync(f => f.User1.Id == user2Id && f.User2.Id == user1Id);

                return friendship;
            }
        }
    }
}
