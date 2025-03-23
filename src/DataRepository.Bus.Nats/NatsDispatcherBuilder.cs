using DataRepository.Bus.Serialization;
using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using NATS.Net;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace DataRepository.Bus.Nats
{
    public class NatsDispatcherBuilder : DispatcherBuilderBase<NatsConsumerIdentity, NatsRequestReplyIdentity>
    {
        public NatsDispatcherBuilder(ILoggerFactory loggerFactory, IMessageSerialization messageSerialization, INatsConnection connection)
        {
            LoggerFactory = loggerFactory;
            MessageSerialization = messageSerialization;
            Connection = connection;
            NatsJSContext = connection.CreateJetStreamContext();
        }

        public INatsJSContext NatsJSContext { get; }

        public ILoggerFactory LoggerFactory { get; }

        public IMessageSerialization MessageSerialization { get; }

        public INatsConnection Connection { get; }

        public NatsDispatcherBuilder AddConsumer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TMessage>(
            string subject, StreamConfig streamConfig, ConsumerConfig consumerConfig, NatsJSNextOpts? natsJSNextOpts = null, bool parallelConsumer = true, uint scale = 1)
        {
            streamConfigMap[typeof(TMessage)] = new NatsConsumerIdentity(subject, streamConfig, consumerConfig, natsJSNextOpts, typeof(TMessage), parallelConsumer, scale);
            return this;
        }

        public NatsDispatcherBuilder AddConsumer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TMessage>(
            string subject, string id, NatsJSNextOpts? natsJSNextOpts = null, bool parallelConsumer = true, uint scale = 1)
        {
            var streamConfig = new StreamConfig(subject, [$"{subject}.*"]);
            var consumerConfig = new ConsumerConfig(id);
            return AddConsumer<TMessage>($"{subject}.{id}", streamConfig, consumerConfig, natsJSNextOpts, parallelConsumer, scale);
        }

        public override async Task<IReadOnlyDictionary<Type, IConsumerDispatcher>> BuildConsumersAsync(CancellationToken token = default)
        {
            var result = new Dictionary<Type, IConsumerDispatcher>();
            foreach (var item in streamConfigMap)
            {
                var stream = await NatsJSContext.CreateOrUpdateStreamAsync(item.Value.StreamConfig, token);
                var dispatcher = new NatsConsumerDispatcher(item.Value, stream, LoggerFactory.CreateLogger($"Consumer<{item.Key.FullName}>"), MessageSerialization);
                result[item.Key] = dispatcher;
            }
            return result.ToFrozenDictionary();
        }

        public NatsDispatcherBuilder AddRequestReply<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TRequest, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TReply>(
            string subject, string id, string? queueGroup = null, NatsSubOpts? natsSubOpts = null, uint scale = 1)
        {
            var requestType = typeof(TRequest);
            var replyType = typeof(TReply);
            queueGroup = subject;
            requestReplyConfigMap[new RequestReplyIdentity(requestType, replyType)] =
                new NatsRequestReplyIdentity(requestType, replyType, $"{subject}.*", $"{subject}.{id}", queueGroup, natsSubOpts, scale);
            return this;
        }

        public override Task<IReadOnlyDictionary<RequestReplyIdentity, IRequestReplyDispatcher>> BuildRequestReplysAsync(CancellationToken token = default)
        {
            var result = new Dictionary<RequestReplyIdentity, IRequestReplyDispatcher>();
            foreach (var item in requestReplyConfigMap)
            {
                var logger = LoggerFactory.CreateLogger($"RequestReply<{item.Key.Request.FullName}, {item.Key.Reply.FullName}>");
                var dispatcher = new NatsRequestReplyDispatcher(Connection, logger, MessageSerialization, item.Value);
                result[item.Key] = dispatcher;
            }
            return Task.FromResult<IReadOnlyDictionary<RequestReplyIdentity, IRequestReplyDispatcher>>(result.ToFrozenDictionary());
        }
    }
}
