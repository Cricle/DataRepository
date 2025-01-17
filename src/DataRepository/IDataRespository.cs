using System.Data;

namespace DataRepository
{
    public interface IDbConnectionProvider
    {
        bool SupportDbConnection { get; }

        IDbConnection? GetConnection();
    }
    public interface IDataRespository<TEntity> : IDataOperatorRespository<TEntity>, IDataBatchRespository<TEntity>, IQueryScope<TEntity>, IDbConnectionProvider
           where TEntity : class
    {
        Task<IDataTransaction> BeginTransactionAsync(IsolationLevel level = IsolationLevel.ReadCommitted, CancellationToken token = default);
    }
}
