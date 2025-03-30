namespace DataRepository.Bus
{
    public interface IConsumerDispatcher : IDataDispatcher<IConsumerIdentity, IReadOnlyList<IBatchConsumer>>
    {
    }

}
