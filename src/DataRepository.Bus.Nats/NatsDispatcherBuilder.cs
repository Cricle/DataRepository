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
        
        public NatsDispatcherBuilder AddConsumer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TMessage>(NatsConsumerIdentity identity)
        {
            consumerConfigMap[typeof(TMessage)] = identity;
            return this;
        }
        public NatsDispatcherBuilder AddConsumer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TMessage>(
            string subject, StreamConfig streamConfig, ConsumerConfig consumerConfig, NatsJSNextOpts? natsJSNextOpts = null, bool parallelConsumer = true, uint scale = 1, uint batchSize=1, bool concurrentHandle=false, TimeSpan? fetchTime=null)
        {
            var identity=new NatsConsumerIdentity(subject, streamConfig, consumerConfig, natsJSNextOpts, typeof(TMessage), parallelConsumer, scale, batchSize, concurrentHandle, fetchTime);
            return AddConsumer<TMessage>(identity);
        }

        public NatsDispatcherBuilder AddConsumer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TMessage>(
            string subject, string id, NatsJSNextOpts? natsJSNextOpts = null, bool parallelConsumer = true, uint scale = 1, uint batchSize = 1, bool concurrentHandle = false, TimeSpan? fetchTime = null)
        {
            var streamConfig = new StreamConfig(subject, [$"{subject}.*"]);
            var consumerConfig = new ConsumerConfig(id);
            return AddConsumer<TMessage>($"{subject}.{id}", streamConfig, consumerConfig, natsJSNextOpts, parallelConsumer, scale, batchSize, concurrentHandle, fetchTime);
        }

        public override async Task<IReadOnlyDictionary<Type, IConsumerDispatcher>> BuildConsumersAsync(CancellationToken token = default)
        {
            var result = new Dictionary<Type, IConsumerDispatcher>();
            foreach (var item in consumerConfigMap)
            {
                var stream = await NatsJSContext.CreateOrUpdateStreamAsync(item.Value.StreamConfig, token);
                var dispatcher = new NatsConsumerDispatcher(item.Value, stream, LoggerFactory.CreateLogger($"Consumer<{item.Key.FullName}>"), MessageSerialization);
                result[item.Key] = dispatcher;
            }
            return result.ToFrozenDictionary();
        }
        public NatsDispatcherBuilder AddRequestReply<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TRequest, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TReply>(NatsRequestReplyIdentity identity)
        {
            requestReplyConfigMap[new RequestReplyPair(typeof(TRequest), typeof(TReply))] = identity;
            return this;
        }
        public NatsDispatcherBuilder AddRequestReply<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TRequest, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TReply>(
            string subject, string id, string? queueGroup = null, NatsSubOpts? natsSubOpts = null, uint scale = 1, bool concurrentHandle = false)
        {
            if (scale != 1 && queueGroup == null)
            {
                queueGroup = subject;
            }
            return AddRequestReply<TRequest,TReply>(new NatsRequestReplyIdentity(typeof(TRequest), typeof(TReply), $"{subject}.*", $"{subject}.{id}", queueGroup, natsSubOpts, scale, concurrentHandle));
        }

        public override Task<IReadOnlyDictionary<RequestReplyPair, IRequestReplyDispatcher>> BuildRequestReplysAsync(CancellationToken token = default)
        {
            var result = new Dictionary<RequestReplyPair, IRequestReplyDispatcher>();
            foreach (var item in requestReplyConfigMap)
            {
                var logger = LoggerFactory.CreateLogger($"RequestReply<{item.Key.Request.FullName}, {item.Key.Reply.FullName}>");
                var dispatcher = new NatsRequestReplyDispatcher(Connection, logger, MessageSerialization, item.Value);
                result[item.Key] = dispatcher;
            }
            return Task.FromResult<IReadOnlyDictionary<RequestReplyPair, IRequestReplyDispatcher>>(result.ToFrozenDictionary());
        }

    }
}
