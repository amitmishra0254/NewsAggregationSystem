using NewsAggregationSystem.DAL.DbContexts;
using System.Linq.Expressions;

namespace NewsAggregationSystem.DAL.Repositories.Generic
{
    public interface IRepositoryBase<TEntity> where TEntity : class
    {
        IQueryable<TEntity> GetAll();
        IQueryable<TEntity> GetWhere(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity> GetSingleAsync(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity> GetSingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
        NewsAggregationSystemContext GetDbContext();
        Task<int> AddAsync(TEntity entity);
        Task<int> AddRangeAsync(List<TEntity> entity);
        Task<int> DeleteAsync(TEntity entity);
        Task<int> UpdateAsync(TEntity entity);
        Task<int> UpdateRangeAsync(List<TEntity> entities);
        Task<int> SaveAsync();
    }
}
