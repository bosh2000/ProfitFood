using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.DAL.Repository.Interfaces
{
    public interface IRepositoryBase<T> where T : class
    {
        Task<List<T>> ConditionToListAsync(Expression<Func<T, bool>> predicate, bool disableTracking = true);

        Task<List<T>> ToListAsync();

        Task<T> FirstOfDefaultAsync(Expression<Func<T, bool>> expression);

        Task<T> CreateASync(T entity);

        Task UpdateAsync(T entity);

        Task DeleteAsync(T entity);

        Task SaveAsync();
    }
}