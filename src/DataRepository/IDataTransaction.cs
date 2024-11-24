namespace DataRepository
{
    public interface IDataTransaction : IDisposable
    {
        Guid TransactionId { get; }

        Task CommitAsync(CancellationToken cancellationToken = default);

        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}
