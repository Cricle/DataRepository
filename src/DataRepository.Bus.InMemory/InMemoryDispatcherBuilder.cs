using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;

namespace DataRepository.Bus.InMemory
{
    public class InMemoryDispatcherBuilder : DispatcherBuilderBase<InMemoryConsumerIdentity, InMemoryRequestReplyIdentity>
    {
        public InMemoryDispatcherBuilder(ILoggerFactory loggerFactory)
        {
            LoggerFactory = loggerFactory;
        }

        public ILoggerFactory LoggerFactory { get; }

        public InMemoryDispatcherBuilder AddConsumer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TMessage>(InMemoryConsumerIdentity identity)
        {
            consumerConfigMap[typeof(TMessage)] = identity;
            return this;
        }

        public InMemoryDispatcherBuilder AddConsumerUnBound<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TMessage>(
             bool concurrentHandle = true, bool parallelConsumer = true, uint scale = 1, uint batchSize = 1, TimeSpan? fetchTime = null)
        {
            return AddConsumer<TMessage>(new InMemoryConsumerIdentity(typeof(TMessage), concurrentHandle, parallelConsumer, true, new UnboundedChannelOptions { SingleReader = scale == 1 }, null, batchSize, fetchTime, scale));
        }

        public InMemoryDispatcherBuilder AddConsumerBound<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TMessage>(
             int capacity, bool concurrentHandle = true, bool parallelConsumer = true, uint scale = 1, uint batchSize = 1, TimeSpan? fetchTime = null)
        {
            return AddConsumer<TMessage>(new InMemoryConsumerIdentity(typeof(TMessage), concurrentHandle, parallelConsumer, false, null, new BoundedChannelOptions(capacity) { SingleReader = scale == 1, FullMode = BoundedChannelFullMode.Wait }, batchSize, fetchTime, scale));
        }

        public InMemoryDispatcherBuilder AddRequestReply<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TRequest, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TReply>(
            InMemoryRequestReplyIdentity identity)
        {
            requestReplyConfigMap[new RequestReplyPair(typeof(TRequest), typeof(TReply))] = identity;
            return this;
        }

        public InMemoryDispatcherBuilder AddRequestReplyUnBound<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TRequest, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TReply>(
            bool concurrentHandle = false, uint scale = 1)
        {
            return AddRequestReply<TRequest, TReply>(new InMemoryRequestReplyIdentity(typeof(TRequest), typeof(TReply), concurrentHandle, true, new UnboundedChannelOptions { SingleReader = scale == 1 }, null, scale));
        }

        public InMemoryDispatcherBuilder AddRequestReplyBound<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TRequest, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TReply>(
            int capacity, bool concurrentHandle = true, uint scale = 1)
        {
            return AddRequestReply<TRequest, TReply>(new InMemoryRequestReplyIdentity(typeof(TRequest), typeof(TReply), concurrentHandle, false, null, new BoundedChannelOptions(capacity) { SingleReader = scale == 1, FullMode = BoundedChannelFullMode.Wait }, scale));
        }

        public override Task<IReadOnlyDictionary<Type, IConsumerDispatcher>> BuildConsumersAsync(CancellationToken token = default)
        {
            var res = new Dictionary<Type, IConsumerDispatcher>();
            foreach (var item in consumerConfigMap)
            {
                var logger = LoggerFactory.CreateLogger($"Consumer <{item.Key}>");
                res[item.Key] = new InMemoryConsumerDispatcher(item.Value, logger);
            }

            return Task.FromResult<IReadOnlyDictionary<Type, IConsumerDispatcher>>(res);
        }

        public override Task<IReadOnlyDictionary<RequestReplyPair, IRequestReplyDispatcher>> BuildRequestReplysAsync(CancellationToken token = default)
        {
            var res = new Dictionary<RequestReplyPair, IRequestReplyDispatcher>();
            foreach (var item in requestReplyConfigMap)
            {
                var logger = LoggerFactory.CreateLogger($"RequestReply <{item.Key.Request},{item.Key.Reply}>");
                res[item.Key] = new InMemoryRequestReplyDispatcher(item.Value, logger);
            }

            return Task.FromResult<IReadOnlyDictionary<RequestReplyPair, IRequestReplyDispatcher>>(res);
        }
    }
}
