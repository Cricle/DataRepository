namespace DataRepository.Bus
{
    public abstract class DispatcherBuilderBase<TConsumerIdentity, TRequestReplyIdentity> : IDispatcherBuilder
        where TConsumerIdentity : class, IConsumerIdentity
        where TRequestReplyIdentity : class, IRequestReplyIdentity
    {
        protected internal readonly Dictionary<Type, TConsumerIdentity> consumerConfigMap = new Dictionary<Type, TConsumerIdentity>();
        protected internal readonly Dictionary<RequestReplyPair, TRequestReplyIdentity> requestReplyConfigMap = new Dictionary<RequestReplyPair, TRequestReplyIdentity>();

        public abstract Task<IReadOnlyDictionary<Type, IConsumerDispatcher>> BuildConsumersAsync(CancellationToken token = default);

        public abstract Task<IReadOnlyDictionary<RequestReplyPair, IRequestReplyDispatcher>> BuildRequestReplysAsync(CancellationToken token = default);

        public virtual IConsumerIdentity? GetConsumerIdentity(Type type)
        {
            return consumerConfigMap.TryGetValue(type, out var identity) ? identity : null;
        }

        public virtual IRequestReplyIdentity? GetRequestReplyIdentity(RequestReplyPair identity)
        {
            return requestReplyConfigMap.TryGetValue(identity, out var rr) ? rr : null;
        }
    }
}
