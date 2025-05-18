using System.Data;
#if !NETSTANDARD
using System.Diagnostics.CodeAnalysis;
#endif

namespace DataRepository
{
    public interface IDataRespository<
#if !NETSTANDARD
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
    TEntity> : IDataOperatorRespository<TEntity>, IDataBatchRespository<TEntity>, IQueryScope<TEntity>, IDbConnectionProvider
           where TEntity : class
    {
        Task<IDataTransaction> BeginTransactionAsync(IsolationLevel level = IsolationLevel.ReadCommitted, CancellationToken token = default);
    }
}
