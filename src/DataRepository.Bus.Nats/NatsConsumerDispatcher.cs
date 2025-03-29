using DataRepository.Bus.Serialization;
using Microsoft.Extensions.Logging;
using NATS.Client.JetStream;
using System.Runtime.CompilerServices;

namespace DataRepository.Bus.Nats
{
    internal class NatsConsumerDispatcher : ConsumerDispatcherBase<INatsJSConsumer>
    {
        public NatsConsumerDispatcher(NatsConsumerIdentity identity, INatsJSStream stream, ILogger logger, IMessageSerialization messageSerialization) : base(logger)
        {
            Identity = natsConsumerIdentity = identity;
            Stream = stream;
            Logger = logger;
            MessageSerialization = messageSerialization;
        }

        private readonly NatsConsumerIdentity natsConsumerIdentity;

        public INatsJSStream Stream { get; }

        public override IConsumerIdentity Identity { get; }

        public ILogger Logger { get; }

        public IMessageSerialization MessageSerialization { get; }

        protected override async Task<INatsJSConsumer?> StartingCreateContextAsync(IReadOnlyList<IConsumer> context, CancellationToken token = default)
        {
            return await Stream.CreateOrUpdateConsumerAsync(natsConsumerIdentity.ConsumerConfig, token).ConfigureAwait(false);
        }

        protected override async Task<object?> ReadSingleAsync(INatsJSConsumer? context, CancellationToken token)
        {
            var msg = await context!.NextAsync<ReadOnlyMemory<byte>>(cancellationToken: token).ConfigureAwait(false);
            if (msg == null) return null;
            return MessageSerialization.FromBytes(msg.Value.Data, Identity.MessageType);
        }

        protected override async IAsyncEnumerable<object?> ReadAsync(INatsJSConsumer? context, [EnumeratorCancellation] CancellationToken token)
        {
            await foreach (var item in context!.ConsumeAsync<ReadOnlyMemory<byte>>(cancellationToken: token).ConfigureAwait(false))
            {
                yield return MessageSerialization.FromBytes(item.Data, Identity.MessageType);
            }
        }
    }
}
