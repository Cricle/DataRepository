using System.Data;

namespace DataRepository
{
    public interface IDataRespository<TEntity> : IDataOperatorRespository<TEntity>, IDataBatchRespository<TEntity>, IQueryScope<TEntity>
           where TEntity : class
    {
        Task<IDataTransaction> BeginTransactionAsync(IsolationLevel level = IsolationLevel.ReadCommitted, CancellationToken token = default);
    }
}
