namespace DataRepository.Bus
{
    public interface IConsumerIdentity
    {
        Type MessageType { get; }

        uint Scale { get; }

        bool ParallelConsumer { get; }
    }
}
