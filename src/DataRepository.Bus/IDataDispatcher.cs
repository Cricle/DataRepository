namespace DataRepository.Bus
{
    public interface IDataDispatcher<TIdentity, TContext>
    {
        TIdentity Identity { get; }

        Task LoopReceiveAsync(TContext context, CancellationToken token = default);
    }
}
