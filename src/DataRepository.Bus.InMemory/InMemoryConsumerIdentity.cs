using System.Threading.Channels;

namespace DataRepository.Bus.InMemory
{
    public class InMemoryConsumerIdentity : ChannelIdentity, IConsumerIdentity
    {
        public InMemoryConsumerIdentity(Type messageType,
            bool concurrentHandle, 
            bool parallelConsumer, 
            bool unBoundChannel, 
            UnboundedChannelOptions? unBoundedChannelOptions, 
            BoundedChannelOptions? boundedChannelOptions, 
            uint batchSize, 
            TimeSpan? fetchTime)
            : base(unBoundChannel, unBoundedChannelOptions, boundedChannelOptions)
        {
            MessageType = messageType;
            ConcurrentHandle = concurrentHandle;
            ParallelConsumer = parallelConsumer;
            BatchSize = batchSize;
            FetchTime = fetchTime;
        }

        public Type MessageType { get; }

        uint IConsumerIdentity.Scale { get; } = 1;

        public bool ConcurrentHandle { get; }

        public bool ParallelConsumer { get; }

        public uint BatchSize { get; }

        public TimeSpan? FetchTime { get; }
    }
}
