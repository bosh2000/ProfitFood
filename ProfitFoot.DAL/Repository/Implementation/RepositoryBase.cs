using Microsoft.EntityFrameworkCore;
using ProfitFood.DAL.Repository.Interfaces;
using ProfitFoot.Infrastructure;
using System.Linq.Expressions;

namespace ProfitFood.Infrastructure.Repository.Implementation
{
    /// <summary>
    /// Базовый репозиторий, который работает с короткоживущим <see cref="ProfitFoodDbContext"/>,
    /// создаваемым на каждую операцию.
    /// </summary>
    /// <typeparam name="T">Тип сущности, с которым работает репозиторий.</typeparam>
    public class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        private readonly IDbContextFactory<ProfitFoodDbContext> _contextFactory;

        /// <summary>
        /// Инициализирует репозиторий фабрикой, которая создаёт контекст базы данных по требованию.
        /// </summary>
        /// <param name="contextFactory">Фабрика, выдающая новый контекст на каждую операцию.</param>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="contextFactory"/> равно null.</exception>
        protected RepositoryBase(IDbContextFactory<ProfitFoodDbContext> contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        // Окна десктопного приложения живут часами, а DbContext не потокобезопасен и держит все
        // загруженные сущности в трекере изменений. Контекст на операцию снимает обе проблемы.
        private Task<ProfitFoodDbContext> CreateContextAsync(CancellationToken cancellationToken = default)
            => _contextFactory.CreateDbContextAsync(cancellationToken);

        /// <summary>Возвращает сущности, удовлетворяющие условию.</summary>
        /// <param name="predicate">Выражение фильтрации.</param>
        /// <param name="disableTracking">Если false, сущности материализуются с AsNoTracking().</param>
        /// <returns>Список подходящих сущностей.</returns>
        public async Task<List<T>> ConditionToListAsync(Expression<Func<T, bool>> predicate, bool disableTracking = true)
        {
            await using var context = await CreateContextAsync();
            IQueryable<T> query = context.Set<T>();

            if (!disableTracking)
                query = query.Where(predicate).AsNoTracking();
            else
                query = query.Where(predicate);

            return await query.ToListAsync();
        }

        /// <summary>Добавляет сущность и сразу сохраняет её в базе данных.</summary>
        /// <param name="entity">Сущность для добавления.</param>
        /// <returns>Добавленная сущность со сгенерированными значениями.</returns>
        public async Task<T> CreateASync(T entity)
        {
            await using var context = await CreateContextAsync();
            var result = await context.Set<T>().AddAsync(entity);
            await context.SaveChangesAsync();
            return result.Entity;
        }

        /// <summary>Удаляет сущность и сразу сохраняет изменение в базе данных.</summary>
        /// <param name="entity">Сущность для удаления.</param>
        public async Task DeleteAsync(T entity)
        {
            await using var context = await CreateContextAsync();
            context.Set<T>().Remove(entity);
            await context.SaveChangesAsync();
        }

        /// <summary>Возвращает первую сущность, подходящую под выражение, или null.</summary>
        /// <param name="expression">Выражение фильтрации.</param>
        /// <returns>Найденная сущность или null.</returns>
        public async Task<T> FirstOfDefaultAsync(Expression<Func<T, bool>> expression)
        {
            await using var context = await CreateContextAsync();
            return (await context.Set<T>().FirstOrDefaultAsync(expression))!;
        }

        /// <summary>Возвращает все сущности набора.</summary>
        /// <returns>Все хранящиеся сущности.</returns>
        public async Task<List<T>> ToListAsync()
        {
            await using var context = await CreateContextAsync();
            return await context.Set<T>().ToListAsync();
        }

        /// <summary>Присоединяет сущность как изменённую и сразу сохраняет её в базе данных.</summary>
        /// <param name="entity">Сущность с новыми значениями.</param>
        public async Task UpdateAsync(T entity)
        {
            await using var context = await CreateContextAsync();
            context.Set<T>().Update(entity);
            await context.SaveChangesAsync();
        }
    }
}
