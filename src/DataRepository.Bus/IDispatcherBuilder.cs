namespace DataRepository.Bus
{
    public interface IDispatcherBuilder
    {
        IConsumerIdentity? GetConsumerIdentity(Type type);

        IRequestReplyIdentity? GetRequestReplyIdentity(RequestReplyIdentity identity);

        Task<IReadOnlyDictionary<Type, IConsumerDispatcher>> BuildConsumersAsync(CancellationToken token = default);

        Task<IReadOnlyDictionary<RequestReplyIdentity, IRequestReplyDispatcher>> BuildRequestReplysAsync(CancellationToken token = default);
    }
}
