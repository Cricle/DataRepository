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
        }

        public override IConsumerIdentity Identity { get; }

        public Channel<object> Channel { get; }

        protected override async Task<object?> ReadSingleAsync(object? _, CancellationToken token) => await Channel.Reader.ReadAsync(token);

        protected override long GetPenddingMessageCount() => Channel.Reader.Count;
    }
}
