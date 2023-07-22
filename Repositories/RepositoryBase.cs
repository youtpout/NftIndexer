using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NftIndexer.Entities;
using NftIndexer.Interfaces;

namespace NftIndexer.Repositories
{
	public class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {

        private readonly NftIndexerContext _dbContext;
        private DbSet<T> entities;

        public RepositoryBase(NftIndexerContext dbContext)
        {
            _dbContext = dbContext;
            entities = dbContext.Set<T>();
        }

        public async virtual Task<bool> Create(T entity)
        {
            await entities.AddAsync(entity);
            return await Save();
        }

        public async virtual Task<bool> Delete(T entity)
        {
            entities.Remove(entity);
            return await Save();
        }

        public async virtual Task<IList<T>> FindAll()
        {
            var refs = await entities.ToListAsync();
            return refs;
        }

        public async virtual Task<T> FindById(int id)
        {
            var ent = await entities.FindAsync(id);
            return ent;
        }

        public async virtual Task<T> FindWithParameter(Expression<Func<T,bool>> parameter)
        {
            var ent = await entities.FirstOrDefaultAsync(parameter);
            return ent;
        }

        public virtual IQueryable<T> GetAsQueryable()
        {
            var ent = entities.AsQueryable();
            return ent;
        }

        public async virtual Task<bool> Save()
        {
            var changes = await _dbContext.SaveChangesAsync();
            return changes > 0;
        }

        public async virtual Task<bool> Update(T entity)
        {
            entities.Update(entity);
            return await Save();
        }
    }
}

