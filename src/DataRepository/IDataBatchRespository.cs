#if !NETSTANDARD
using System.Diagnostics.CodeAnalysis;
#endif
using System.Linq.Expressions;

namespace DataRepository
{
    public interface IDataBatchRespository<
#if !NETSTANDARD
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
    TEntity>
    {
        int InsertMany(IEnumerable<TEntity> entities);

        Task<int> InsertManyAsync(IEnumerable<TEntity> entities, CancellationToken token = default);

        int DeleteMany(IEnumerable<TEntity> entities);

        Task<int> DeleteManyAsync(IEnumerable<TEntity> entities, CancellationToken token = default);

        int UpdateMany(IEnumerable<TEntity> entities);

        Task<int> UpdateManyAsync(IEnumerable<TEntity> entities, CancellationToken token = default);

        int ExecuteUpdate(Expression expression);

        Task<int> ExecuteUpdateAsync(Expression expression, CancellationToken token = default);

        int ExecuteDelete();

        Task<int> ExecuteDeleteAsync(CancellationToken token = default);

        IUpdateSetBuilder<TEntity> CreateUpdateBuilder();
    }
}
