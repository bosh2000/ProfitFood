using Microsoft.EntityFrameworkCore;
using ProfitFood.DAL.Repository.Interfaces;
using ProfitFoot.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.DAL.Repository.Implementation
{
    internal class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        private readonly ProfitFoodDbContext _dbContext;
        private readonly DbSet<T> _dbSet;

        public RepositoryBase(ProfitFoodDbContext context)
        {
            _dbContext = context;
            _dbSet = context.Set<T>();
        }

        public Task<List<T>> ConditionToListAsync(Expression<Func<T, bool>> predicate, bool disableTracking = true)
        {
            _dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
            IQueryable<T> query = _dbSet;
            if (!disableTracking)
                query = query.Where(predicate).AsNoTracking();
            else
                query = query.Where(predicate);
            return query.ToListAsync();
        }

        public async Task<T> CreateASync(T entity)
        {
            var result = await _dbSet.AddAsync(entity);
            await SaveAsync();
            return result.Entity;
        }

        public async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await SaveAsync();
        }

        public async Task<T> FirstOfDefaultAsync(Expression<Func<T, bool>> expression) => await _dbSet.FirstOrDefaultAsync(expression);

        public Task SaveAsync() => _dbContext.SaveChangesAsync();

        public Task<List<T>> ToListAsync()
        {
            return _dbSet.ToListAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            _dbContext.ChangeTracker.AutoDetectChangesEnabled = true;
            _dbSet.Attach(entity).State = EntityState.Modified;
            _dbSet.Update(entity);
            await _dbContext.SaveChangesAsync();
            _dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
        }
    }
}