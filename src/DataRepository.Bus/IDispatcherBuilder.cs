namespace DataRepository.Bus
{
    public interface IDispatcherBuilder
    {
        IConsumerIdentity? GetConsumerIdentity(Type type);

        IRequestReplyIdentity? GetRequestReplyIdentity(RequestReplyPair identity);

        Task<IReadOnlyDictionary<Type, IConsumerDispatcher>> BuildConsumersAsync(CancellationToken token = default);

        Task<IReadOnlyDictionary<RequestReplyPair, IRequestReplyDispatcher>> BuildRequestReplysAsync(CancellationToken token = default);
    }
}
