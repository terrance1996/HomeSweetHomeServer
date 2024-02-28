using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeSweetHomeServer.Repositories
{
    //Interface about base repository operations
    public interface IBaseRepository<TEntity> where TEntity : class 
    {
        Task InsertAsync(TEntity entity);
        Task DeleteAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        void Insert(TEntity entity);
        void Update(TEntity entity);
        void Delete(TEntity entity);
    }
}
