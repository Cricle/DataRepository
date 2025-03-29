using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace DataRepository.Bus.InMemory
{
    public sealed class InMemoryConsumerDispatcher : ConsumerDispatcherBase<object>
    {
        public InMemoryConsumerDispatcher(InMemoryConsumerIdentity identity, ILogger logger)
            : base(logger)
        {
            Identity = identity;
            Channel = identity.CreateChannel<object>();
            Logger = logger;
        }

        public override IConsumerIdentity Identity { get; }

        public Channel<object> Channel { get; }

        public ILogger Logger { get; }

        protected override async Task<object?> ReadSingleAsync(object? _, CancellationToken token) => await Channel.Reader.ReadAsync(token);
    }
}
