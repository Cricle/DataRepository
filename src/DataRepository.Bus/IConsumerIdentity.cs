namespace DataRepository.Bus
{
    public interface IConsumerIdentity
    {
        Type MessageType { get; }

        uint BatchSize { get; }

        uint Scale { get; }

        bool ParallelConsumer { get; }

        bool ConcurrentHandle { get; }

        TimeSpan? FetchTime { get; } 
    }
}
