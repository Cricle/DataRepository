using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace DataRepository.Bus.InMemory
{
    public class InMemoryBus : BatchBusBase<InMemoryConsumerDispatcher,InMemoryRequestReplyDispatcher>
    {
        public InMemoryBus(IServiceScopeFactory serviceScopeFactory, ILogger<InMemoryBus> logger, IDispatcherBuilder dispatcherBuilder)
            : base(serviceScopeFactory, logger, dispatcherBuilder)
        {
        }

        public override Task PublishAsync<TMessage>(TMessage message, IDictionary<string, object?>? header = null, CancellationToken cancellationToken = default)
        {
            ThrowIfIdentitiesNull();
            return base.PublishAsync(message, header, cancellationToken);
        }

        public override Task<TReply> RequestAsync<TRequest, TReply>(TRequest message, IDictionary<string, object?>? header = null, CancellationToken cancellationToken = default)
        {
            ThrowIfIdentitiesNull();
            return base.RequestAsync<TRequest, TReply>(message, header, cancellationToken);
        }

        protected override async Task CorePublishAsync<TMessage>(TMessage message, IConsumerIdentity identity, IDictionary<string, object?>? header = null, CancellationToken cancellationToken = default)
        {
            var dispatcher = consumerIdentities![typeof(TMessage)];
            await dispatcher.Channel.Writer.WriteAsync(message!, cancellationToken).ConfigureAwait(false);
        }

        protected override async Task<TReply> CoreRequestAsync<TRequest, TReply>(TRequest request, IRequestReplyIdentity identity, IDictionary<string, object?>? header = null, CancellationToken cancellationToken = default)
        {
            var dispatcher = requestReplyIdentities![identity.RequestReplyPair];
            var requestReplyBox = new RequestReplyBox(request!);
            await dispatcher.Channel.Writer.WriteAsync(requestReplyBox, cancellationToken);
            return (TReply)await requestReplyBox.Task;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfIdentitiesNull()
        {
            if (consumerIdentities == null) throw new InvalidOperationException("In memory bus must all in started");
        }
    }
}
