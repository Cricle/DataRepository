namespace DataRepository.Bus
{
    //https://github.com/zarusz/SlimMessageBus/
    public interface IBus : IDisposable, IAsyncDisposable
    {
        bool IsStarted { get; }

        Task StartAsync(CancellationToken cancellationToken = default);

        Task StopAsync(CancellationToken cancellationToken = default);

        Task PublishAsync<TMessage>(TMessage message, IDictionary<string, object?>? header = null, CancellationToken cancellationToken = default);

        Task PublishManyAsync<TMessage>(IEnumerable<TMessage> messages, Func<TMessage, IDictionary<string, object?>?>? headerFactory = null, CancellationToken cancellationToken = default);

        Task<TReply> RequestAsync<TRequest,TReply>(TRequest message, IDictionary<string, object?>? header = null, CancellationToken cancellationToken = default);
    }
}
