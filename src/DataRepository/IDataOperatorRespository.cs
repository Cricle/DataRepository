#if !NETSTANDARD
using System.Diagnostics.CodeAnalysis;
#endif

namespace DataRepository
{
    public interface IDataOperatorRespository<
#if !NETSTANDARD
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
    TEntity>
    {
        int Insert(TEntity entity);

        Task<int> InsertAsync(TEntity entity, CancellationToken token = default);

        int Update(TEntity entity);

        Task<int> UpdateAsync(TEntity entity, CancellationToken token = default);

        int Delete(TEntity entity);

        Task<int> DeleteAsync(TEntity entity, CancellationToken token = default);
    }
}
