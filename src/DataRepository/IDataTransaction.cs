using System.Data;

namespace DataRepository
{
    public interface IDataTransaction : IDbTransaction
    {
        Guid TransactionId { get; }

        Task CommitAsync(CancellationToken cancellationToken = default);

        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}
