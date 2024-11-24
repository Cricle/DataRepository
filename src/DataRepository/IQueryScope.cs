using System.Linq.Expressions;

namespace DataRepository
{
    public interface IQueryScope<TEntity> : IQueryable<TEntity>
        where TEntity : class
    {
        Task<int> CountAsync(CancellationToken token = default);

        Task<long> LongCountAsync(CancellationToken token = default);

        Task<TEntity?> FirstOrDefaultAsync(CancellationToken token = default);

        Task<TEntity?> LastOrDefaultAsync(CancellationToken token = default);

        Task<TEntity[]> ToArrayAsync(CancellationToken token = default);

        Task<List<TEntity>> ToListAsync(CancellationToken token = default);

        IDataRespository<TEntity> Where(Expression<Func<TEntity, bool>> expression);

        IDataRespository<TNewEntity> Select<TNewEntity>(Expression<Func<TEntity, TNewEntity>> expression)
            where TNewEntity : class;
    }
}
