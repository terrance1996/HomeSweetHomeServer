using System.Collections.Generic;
using System.Threading.Tasks;
using HomeSweetHomeServer.Models;
using HomeSweetHomeServer.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HomeSweetHomeServer.Repositories
{
    public class UserInformationRepository : BaseRepository<UserInformationModel>, IUserInformationRepository
    {
        IUserRepository _userRepository;
        IInformationRepository _informationRepository;
        public UserInformationRepository(DatabaseContext context, IUserRepository userRepository, IInformationRepository informationRepository) : base(context)
        {
            _userRepository = userRepository;
            _informationRepository = informationRepository;
        }
        
        //Gets user information by value
        public async Task<UserInformationModel> GetUserInformationByValueAsync(string value, bool include = false)
        {
            if (include == false)
                return await Db.SingleOrDefaultAsync(ui => ui.Value == value);
            else
                return await Db.Include(u => u.User).Include(i => i.Information).SingleOrDefaultAsync(ui => ui.Value == value);
        }

        //Gets all user informations by user id
        public async Task<List<UserInformationModel>> GetAllUserInformationsByUserIdAsync(int userId, bool include = false)
        {
            if (include == false)
                return await Db.Where(ui => ui.User.Id == userId).ToListAsync();
            else
                return await Db.Include(u => u.User).Include(i => i.Information).Where(ui => ui.User.Id == userId).ToListAsync();
        }

        //Gets spesific user information by user id and information id
        public async Task<UserInformationModel> GetUserInformationByIdAsync(int userId, int informationId, bool include = false)
        {
            if (include == false)
                return await Db.SingleOrDefaultAsync(ui => (ui.User.Id == userId && ui.Information.Id == informationId));
            else
                return await Db.Include(u => u.User).Include(i => i.Information).SingleOrDefaultAsync(ui => (ui.User.Id == userId && ui.Information.Id == informationId));
        }
    }
}
