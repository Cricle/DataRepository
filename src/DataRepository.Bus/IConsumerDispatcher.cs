namespace DataRepository.Bus
{
    public interface IDataDispatcher<TIdentity,TContext>
    {
        TIdentity Identity { get; }

        Task LoopReceiveAsync(TContext context, CancellationToken token = default);
    }
    public interface IConsumerDispatcher: IDataDispatcher<IConsumerIdentity, IReadOnlyList<IConsumer>>
    {
    }
    public interface IDispatcherBuilder
    {
        IConsumerIdentity? GetConsumerIdentity(Type type);

        IRequestReplyIdentity? GetRequestReplyIdentity(RequestReplyIdentity identity);

        Task<IReadOnlyDictionary<Type, IConsumerDispatcher>> BuildConsumersAsync(CancellationToken token = default);

        Task<IReadOnlyDictionary<RequestReplyIdentity, IRequestReplyDispatcher>> BuildRequestReplysAsync(CancellationToken token = default);
    }

    public record struct RequestReplyIdentity(Type Request, Type Reply);

    public abstract class DispatcherBuilderBase<TConsumerIdentity, TRequestReplyIdentity> : IDispatcherBuilder
        where TConsumerIdentity : class, IConsumerIdentity
        where TRequestReplyIdentity : class, IRequestReplyIdentity
    {
        protected internal readonly Dictionary<Type, TConsumerIdentity> streamConfigMap = new Dictionary<Type, TConsumerIdentity>();
        protected internal readonly Dictionary<RequestReplyIdentity, TRequestReplyIdentity> requestReplyConfigMap = new Dictionary<RequestReplyIdentity, TRequestReplyIdentity>();

        public abstract Task<IReadOnlyDictionary<Type, IConsumerDispatcher>> BuildConsumersAsync(CancellationToken token = default);

        public abstract Task<IReadOnlyDictionary<RequestReplyIdentity, IRequestReplyDispatcher>> BuildRequestReplysAsync(CancellationToken token = default);

        public virtual IConsumerIdentity? GetConsumerIdentity(Type type)
        {
            return streamConfigMap.TryGetValue(type, out var identity) ? identity : null;
        }

        public virtual IRequestReplyIdentity? GetRequestReplyIdentity(RequestReplyIdentity identity)
        {
            return requestReplyConfigMap.TryGetValue(identity, out var rr) ? rr : null;
        }
    }
}
