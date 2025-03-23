using DataRepository.Bus.Serialization;
using Microsoft.Extensions.Logging;
using NATS.Client.Core;

namespace DataRepository.Bus.Nats
{
    public class NatsRequestReplyDispatcher : IRequestReplyDispatcher
    {
        private readonly INatsConnection connection;
        private readonly ILogger logger;
        private readonly IMessageSerialization messageSerialization;

        public NatsRequestReplyDispatcher(INatsConnection connection,ILogger logger, IMessageSerialization messageSerialization, NatsRequestReplyIdentity identity)
        {
            this.connection = connection;
            this.logger = logger;
            this.messageSerialization = messageSerialization;
            Identity = identity;
        }

        public NatsRequestReplyIdentity Identity { get; }

        IRequestReplyIdentity IDataDispatcher<IRequestReplyIdentity, IRequestReply>.Identity => Identity;

        public async Task LoopReceiveAsync(IRequestReply requestReply, CancellationToken token = default)
        {
            var identity = Identity;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await foreach (var item in connection.SubscribeAsync<ReadOnlyMemory<byte>>(identity.Subject, identity.QueueGroup, null, identity.NatsSubOpts, token))
                    {
                        try
                        {
                            var request = messageSerialization.FromBytes(item.Data, identity.RequestType);
                            var res = await requestReply.RequestAsync(request, token).ConfigureAwait(false);
                            var reply = messageSerialization.ToBytes(res, identity.ReplyType);
                            await item.ReplyAsync(reply, cancellationToken: token).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "{subject}.{group} handle raised error", identity.Subject, identity.QueueGroup);
                        }
                    }
                }
                catch (Exception ex) when (!token.IsCancellationRequested)
                {
                    logger.LogError(ex, "{subject}.{group} raised error", identity.Subject, identity.QueueGroup);
                }
            }
        }
    }
}
