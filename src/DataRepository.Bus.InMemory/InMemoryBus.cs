using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace DataRepository.Bus.InMemory
{
    public class InMemoryBus : BusBase
    {
        private Task[]? consumerTasks;
        private Task[]? requestReplyTasks;
        private CancellationTokenSource? tokenSource;
        private IReadOnlyDictionary<Type, InMemoryConsumerDispatcher>? consumerIdentities;
        private IReadOnlyDictionary<RequestReplyIdentity, InMemoryRequestReplyDispatcher>? requestReplyIdentities;
        private IServiceScope? serviceScope;

        private readonly IServiceScopeFactory serviceScopeFactory;

        public InMemoryBus(IDispatcherBuilder dispatcherBuilder, ILogger<InMemoryBus> logger, IServiceScopeFactory serviceScopeFactory)
        {
            DispatcherBuilder = dispatcherBuilder;
            Logger = logger;
            this.serviceScopeFactory = serviceScopeFactory;
        }

        public IDispatcherBuilder DispatcherBuilder { get; }

        public ILogger<InMemoryBus> Logger { get; }

        public override async Task PublishAsync<TMessage>(TMessage message, IDictionary<string, object?>? header = null, CancellationToken cancellationToken = default)
        {
            if (consumerIdentities == null)
            {
                throw new InvalidOperationException("In memory bus must all in started");
            }
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            if (consumerIdentities.TryGetValue(typeof(TMessage), out var dispatcher))
            {
                await dispatcher.Channel.Writer.WriteAsync(message!, cancellationToken);
                return;
            }
            throw new InvalidOperationException($"No {typeof(TMessage)} handler registed");
        }

        public override async Task<TReply> RequestAsync<TRequest, TReply>(TRequest message, IDictionary<string, object?>? header = null, CancellationToken cancellationToken = default)
        {
            if (requestReplyIdentities == null)
            {
                throw new InvalidOperationException("In memory bus must all in started");
            }
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            var identity = new RequestReplyIdentity(typeof(TRequest), typeof(TReply));
            if (requestReplyIdentities.TryGetValue(identity, out var dispatcher))
            {
                var requestReplyBox = new RequestReplyBox(message);
                await dispatcher.Channel.Writer.WriteAsync(requestReplyBox, cancellationToken);
                return (TReply)await requestReplyBox.ReplySource.Task;
            }
            throw new InvalidOperationException($"No <{typeof(TRequest)},{typeof(TReply)}> handler registed");
        }

        protected override async Task OnStartAsync(CancellationToken cancellationToken = default)
        {
            await OnStopAsync(cancellationToken);

            serviceScope = serviceScopeFactory.CreateScope();
            tokenSource = new CancellationTokenSource();

            await BuildConsumerAsync(tokenSource.Token);
            await BuildRequestReplyAsync(tokenSource.Token);
        }

        private async Task BuildConsumerAsync(CancellationToken token)
        {
            var consumers = await DispatcherBuilder.BuildConsumersAsync(token).ConfigureAwait(false);
            consumerIdentities = consumers.ToDictionary(x => x.Key, x => (InMemoryConsumerDispatcher)x.Value);
            consumerTasks=new Task[consumers.Count];
            var index = 0;
            foreach (var item in consumerIdentities)
            {
                var serviceType = typeof(IConsumer<>).MakeGenericType(item.Key);
                var services = serviceScope!.ServiceProvider.GetServices(serviceType).Cast<IConsumer>().ToArray();

                consumerTasks[index++] = item.Value.LoopReceiveAsync(services, tokenSource.Token);
            }
        }

        private async Task BuildRequestReplyAsync(CancellationToken token)
        {
            var requestReplies = await DispatcherBuilder.BuildRequestReplysAsync(token).ConfigureAwait(false);
            requestReplyIdentities = requestReplies.ToDictionary(x => x.Key, x => (InMemoryRequestReplyDispatcher)x.Value);
            requestReplyTasks = new Task[requestReplyIdentities.Count];
            var index = 0;
            foreach (var item in requestReplyIdentities)
            {
                var serviceType = typeof(IRequestReply<,>).MakeGenericType(item.Key.Request, item.Key.Reply);
                var services = (IRequestReply)serviceScope!.ServiceProvider.GetRequiredService(serviceType);

                requestReplyTasks[index++] = item.Value.LoopReceiveAsync(services, token);
            }
        }

        protected override Task OnStopAsync(CancellationToken cancellationToken = default)
        {
            consumerIdentities = null;
            tokenSource?.Cancel();
            consumerIdentities = null;
            requestReplyIdentities = null;
            consumerTasks = null;
            requestReplyTasks = null;
            serviceScope?.Dispose();
            return Task.CompletedTask;
        }
    }
}
