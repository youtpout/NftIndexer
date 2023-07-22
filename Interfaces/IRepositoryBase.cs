using System;
using System.Linq.Expressions;

namespace NftIndexer.Interfaces
{
    public interface IRepositoryBase<T> where T : class
    {
        Task<IList<T>> FindAll();
        IQueryable<T> GetAsQueryable();
        Task<T> FindById(int id);
        Task<T> FindWithParameter(Expression<Func<T, bool>> parameter);
        Task<bool> Create(T entity);
        Task<bool> Update(T entity);
        Task<bool> Delete(T entity);
        Task<bool> Save();
    }
}

