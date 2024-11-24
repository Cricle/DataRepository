using System.Linq.Expressions;

namespace DataRepository
{
    public interface IQueryScope<TEntity> : IQueryable<TEntity>, IAsyncEnumerable<TEntity>
        where TEntity : class
    {
        IDataRespository<TEntity> Where(Expression<Func<TEntity, bool>> expression);

        IDataRespository<TNewEntity> Select<TNewEntity>(Expression<Func<TEntity, TNewEntity>> expression)
            where TNewEntity : class;
    }
}
