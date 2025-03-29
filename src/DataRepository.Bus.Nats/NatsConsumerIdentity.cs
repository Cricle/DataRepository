using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;

namespace DataRepository.Bus.Nats
{
    public class NatsConsumerIdentity: IConsumerIdentity
    {
        public NatsConsumerIdentity(string subject, StreamConfig streamConfig, ConsumerConfig consumerConfig, NatsJSNextOpts? natsJSNextOpts, Type messageType, bool parallelConsumer, uint scale, uint batchSize, bool concurrentHandle, TimeSpan? fetchTime)
        {
            Subject = subject;
            StreamConfig = streamConfig;
            NatsJSNextOpts = natsJSNextOpts;
            ParallelConsumer = parallelConsumer;
            ConsumerConfig = consumerConfig;
            MessageType = messageType;
            Scale = scale;
            BatchSize = batchSize;
            ConcurrentHandle = concurrentHandle;
            FetchTime = fetchTime;
        }

        public string Subject { get; }

        public StreamConfig StreamConfig { get; }

        public ConsumerConfig ConsumerConfig { get; }

        public NatsJSNextOpts? NatsJSNextOpts { get; }

        public bool ParallelConsumer { get; }

        public Type MessageType { get; }

        public uint Scale { get; }

        public uint BatchSize { get; }

        public bool ConcurrentHandle { get; }

        public TimeSpan? FetchTime { get; }
    }
}
