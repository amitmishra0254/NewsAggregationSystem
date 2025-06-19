using Microsoft.EntityFrameworkCore;
using NewsAggregationSystem.DAL.DbContexts;
using System.Linq.Expressions;

namespace NewsAggregationSystem.DAL.Repositories.Generic
{
    public class RepositoryBase<TEntity> : IRepositoryBase<TEntity> where TEntity : class
    {
        private DbSet<TEntity> dbSet;
        private readonly NewsAggregationSystemContext _dbContext;

        public DbSet<TEntity> DbSet => dbSet;

        public NewsAggregationSystemContext GetDbContext()
        {
            return _dbContext;
        }

        public RepositoryBase(NewsAggregationSystemContext dbContext)
        {
            _dbContext = dbContext;
            dbSet = dbContext.Set<TEntity>();
        }

        public IQueryable<TEntity> GetAll()
        {
            return dbSet;
        }

        public IQueryable<TEntity> GetWhere(Expression<Func<TEntity, bool>> predicate)
        {
            return dbSet.Where(predicate);
        }

        public async Task<TEntity> GetSingleAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await dbSet.SingleAsync(predicate);
        }

        public async Task<TEntity> GetSingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await dbSet.SingleOrDefaultAsync(predicate);
        }

        public async Task<int> AddAsync(TEntity entity)
        {
            await dbSet.AddAsync(entity);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> AddRangeAsync(List<TEntity> entities)
        {
            await dbSet.AddRangeAsync(entities);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(TEntity entity)
        {
            dbSet.Attach(entity);
            _dbContext.Entry(entity).State = EntityState.Modified;
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> UpdateRangeAsync(List<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                dbSet.Attach(entity);
                _dbContext.Entry(entity).State = EntityState.Modified;
            }
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(TEntity entity)
        {
            dbSet.Remove(entity);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> SaveAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }
    }
}