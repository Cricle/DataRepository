using DataRepository.Bus.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using System.Diagnostics.CodeAnalysis;

namespace DataRepository.Bus.Nats
{
    public class NatsBus : BatchBusBase
    {
        private readonly INatsConnection connection;
        private readonly IMessageSerialization messageSerialization;

        public NatsBus(INatsConnection connection,
            IServiceScopeFactory serviceScopeFactory,
            IMessageSerialization messageSerialization,
            ILogger<NatsBus> logger,
            IDispatcherBuilder dispatcherBuilder)
            : base(serviceScopeFactory, logger, dispatcherBuilder)
        {
            this.connection = connection;
            this.messageSerialization = messageSerialization;
        }

        protected override async Task CorePublishAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TMessage>(TMessage message, IConsumerIdentity identity, IDictionary<string, object?>? header = null, CancellationToken cancellationToken = default)
        {
            var msg = new NatsMsg<ReadOnlyMemory<byte>>
            {
                Data = messageSerialization.ToBytes(message),
                Connection = connection,
                Subject = ((NatsConsumerIdentity)identity).Subject
            };

            await connection.PublishAsync(msg, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected override async Task<TReply> CoreRequestAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TRequest, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TReply>(TRequest message, IRequestReplyIdentity identity, IDictionary<string, object?>? header = null, CancellationToken cancellationToken = default)
        {
            var data = messageSerialization.ToBytes(message);
            var result = await connection.RequestAsync<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>(((NatsRequestReplyIdentity)identity).PublishKey, data, cancellationToken: cancellationToken).ConfigureAwait(false);
            return (TReply)messageSerialization.FromBytes(result.Data, identity.ReplyType);
        }
    }
}
