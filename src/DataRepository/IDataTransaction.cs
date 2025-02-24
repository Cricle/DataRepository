using System.Data;

namespace DataRepository
{
    public interface IDataTransaction : IDbTransaction
    {
        Guid TransactionId { get; }

        IDbTransaction? Transaction { get; }

        Task CommitAsync(CancellationToken cancellationToken = default);

        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}
