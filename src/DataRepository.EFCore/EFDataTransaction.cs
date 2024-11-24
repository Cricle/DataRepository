using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics;

namespace DataRepository.EFCore
{
    public class EFDataTransaction : IDataTransaction
    {
        internal EFDataTransaction(IDbContextTransaction transaction)
        {
            Debug.Assert(transaction != null);
            Transaction = transaction;
        }

        public IDbContextTransaction Transaction { get; }

        public Guid TransactionId => Transaction.TransactionId;

        public Task CommitAsync(CancellationToken cancellationToken = default) => Transaction.CommitAsync(cancellationToken);

        public Task RollbackAsync(CancellationToken cancellationToken = default) => Transaction.RollbackAsync(cancellationToken);
    }
}
