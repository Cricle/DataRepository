namespace DataRepository.Bus
{
    public abstract class BusBase : IBus
    {
        private int isStarted = 0;

        public bool IsStarted => Volatile.Read(ref isStarted) == 1;

        public void Dispose()
        {
            StopAsync().GetAwaiter().GetResult();
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await StopAsync();
            GC.SuppressFinalize(this);
        }

        public abstract Task PublishAsync<TMessage>(TMessage message, IDictionary<string, object?>? header = null, CancellationToken cancellationToken = default);

        public virtual async Task PublishManyAsync<TMessage>(IEnumerable<TMessage> messages, Func<TMessage, IDictionary<string, object?>?>? headerFactory = null, CancellationToken cancellationToken = default)
        {
            foreach (var item in messages)
            {
                await PublishAsync(item, headerFactory?.Invoke(item), cancellationToken);
            }
        }

        public abstract Task<TReply> RequestAsync<TRequest, TReply>(TRequest message, IDictionary<string, object?>? header = null, CancellationToken cancellationToken = default);

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (Interlocked.CompareExchange(ref isStarted, 1, 0) == 0)
                await OnStartAsync(cancellationToken);
        }

        protected abstract Task OnStartAsync(CancellationToken cancellationToken = default);

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (Interlocked.CompareExchange(ref isStarted, 0, 1) == 1)
                await OnStopAsync(cancellationToken);
        }

        protected abstract Task OnStopAsync(CancellationToken cancellationToken = default);
    }
}
