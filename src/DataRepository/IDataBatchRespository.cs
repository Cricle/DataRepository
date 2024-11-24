using System.Linq.Expressions;

namespace DataRepository
{
    public interface IDataBatchRespository<TEntity>
    {
        int InsertMany(IEnumerable<TEntity> entities);

        Task<int> InsertManyAsync(IEnumerable<TEntity> entities, CancellationToken token = default);

        int DeleteMany(IEnumerable<TEntity> entities);

        Task<int> DeleteManyAsync(IEnumerable<TEntity> entities, CancellationToken token = default);

        int UpdateMany(IEnumerable<TEntity> entities);

        Task<int> UpdateManyAsync(IEnumerable<TEntity> entities, CancellationToken token = default);

        int UpdateInQuery(Expression expression);

        Task<int> UpdateInQueryAsync(Expression expression, CancellationToken token = default);

        int DeleteInQuery();

        Task<int> DeleteInQueryAsync(CancellationToken token = default);

        IUpdateSetBuilder<TEntity> CreateUpdateBuilder();
    }
}
