using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Diagnostics;

namespace DataRepository.EFCore
{
    public class EFDataTransaction : IDataTransaction
    {
        internal EFDataTransaction(IDbContextTransaction transaction, IsolationLevel isolationLevel)
        {
            Debug.Assert(transaction != null);
            Transaction = transaction;
            IsolationLevel = isolationLevel;
        }

        public IDbContextTransaction Transaction { get; }

        public Guid TransactionId => Transaction.TransactionId;

        IDbConnection? IDbTransaction.Connection => null;

        public IsolationLevel IsolationLevel { get; }

        public void Commit() => Transaction.Commit();

        public Task CommitAsync(CancellationToken cancellationToken = default) => Transaction.CommitAsync(cancellationToken);

        public void Dispose() => Transaction.Dispose();

        public void Rollback() => Transaction.Rollback();

        public Task RollbackAsync(CancellationToken cancellationToken = default) => Transaction.RollbackAsync(cancellationToken);
    }
}
