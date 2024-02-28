using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeSweetHomeServer.Models;
using HomeSweetHomeServer.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace HomeSweetHomeServer.Repositories
{
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
    {
        public DatabaseContext Context;
        public DbSet<TEntity> Db;

        //Constructs database context
        public BaseRepository(DatabaseContext context)
        {
            Context = context;
            Db = Context.Set<TEntity>();
        }

        //Adds given entity to database
        public void Insert(TEntity entity)
        {
            Db.Add(entity);
            Context.SaveChanges();
        }

        public async Task InsertAsync(TEntity entity)
        {
            await Db.AddAsync(entity);
            await Context.SaveChangesAsync();
        }

        //Deletes entity
        public void Delete(TEntity entity)
        {
            Db.Remove(entity);
            Context.SaveChanges();
        }

        public  async Task DeleteAsync(TEntity entity)
        {
            Db.Remove(entity);
            await Context.SaveChangesAsync(); 
        }

        //Updates entity
        public void Update(TEntity entity)
        {
            Db.Update(entity);
            Context.SaveChanges();
        }

        public async Task UpdateAsync(TEntity entity)
        {

            Db.Update(entity);
            await Context.SaveChangesAsync();
        }
    }
}
