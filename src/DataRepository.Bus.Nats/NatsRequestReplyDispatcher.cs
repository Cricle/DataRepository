using DataRepository.Bus.Serialization;
using Microsoft.Extensions.Logging;
using NATS.Client.Core;

namespace DataRepository.Bus.Nats
{
    public class NatsRequestReplyDispatcher : RequestReplyDispatcherBase<NatsMsg<ReadOnlyMemory<byte>>>
    {
        private readonly INatsConnection connection;
        private readonly IMessageSerialization messageSerialization;

        public NatsRequestReplyDispatcher(INatsConnection connection,ILogger logger, IMessageSerialization messageSerialization, NatsRequestReplyIdentity identity)
            :base(logger)
        {
            this.connection = connection;
            this.messageSerialization = messageSerialization;
            Identity = natsIdentity = identity;
        }

        private readonly NatsRequestReplyIdentity natsIdentity;

        public override IRequestReplyIdentity Identity { get; }

        protected override IAsyncEnumerable<NatsMsg<ReadOnlyMemory<byte>>> ReadAsync(CancellationToken token)
            => connection.SubscribeAsync<ReadOnlyMemory<byte>>(natsIdentity.Subject, natsIdentity.QueueGroup, null, natsIdentity.NatsSubOpts, token);

        protected override async Task HandleRequestAsync(IRequestReply requestReply, NatsMsg<ReadOnlyMemory<byte>> outbox, CancellationToken token)
        {
            var request = messageSerialization.FromBytes(outbox.Data, natsIdentity.RequestType);
            var res = await requestReply.RequestAsync(request, token).ConfigureAwait(false);
            var reply = messageSerialization.ToBytes(res, natsIdentity.ReplyType);
            await outbox.ReplyAsync(reply, cancellationToken: token).ConfigureAwait(false);
        }
    }
}
