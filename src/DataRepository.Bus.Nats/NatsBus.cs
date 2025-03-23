using DataRepository.Bus.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NATS.Client.Core;

namespace DataRepository.Bus.Nats
{
    public class NatsBus : BusBase
    {
        private Task[][]? consumerTasks;
        private Task[][]? requestReplyTasks;
        private CancellationTokenSource? runningTokenSouce;
        private IReadOnlyDictionary<Type, IConsumerDispatcher>? consumerIdentities;
        private IReadOnlyDictionary<RequestReplyIdentity, IRequestReplyDispatcher>? requestReplyIdentities;
        private IServiceScope? serviceScope;

        private readonly IDispatcherBuilder natsConsumerBuilder;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly INatsConnection connection;
        private readonly ILogger<NatsBus> logger;
        private readonly IMessageSerialization messageSerialization;

        public NatsBus(INatsConnection connection,
            ILogger<NatsBus> logger,
            IMessageSerialization messageSerialization,
            IServiceScopeFactory serviceScopeFactory,
            IDispatcherBuilder natsConsumerBuilder)
        {
            this.connection = connection;
            this.logger = logger;
            this.messageSerialization = messageSerialization;

            this.serviceScopeFactory = serviceScopeFactory;
            this.natsConsumerBuilder = natsConsumerBuilder;

        }

        public override async Task PublishAsync<TMessage>(TMessage message, IDictionary<string, object?>? header = null, CancellationToken cancellationToken = default)
        {
            var identity = natsConsumerBuilder.GetConsumerIdentity(typeof(TMessage)) as NatsConsumerIdentity;
            if (identity != null)
            {
                var msg = new NatsMsg<ReadOnlyMemory<byte>>
                {
                    Data = messageSerialization.ToBytes(message),
                    Connection = connection,
                    Subject = identity.Subject
                };

                await connection.PublishAsync(msg, cancellationToken: cancellationToken).ConfigureAwait(false);
                return;
            }
            throw new InvalidOperationException($"No regist <{typeof(TMessage)}> consumer");
        }

        public override async Task<TReply> RequestAsync<TRequest, TReply>(TRequest message, IDictionary<string, object?>? header = null, CancellationToken cancellationToken = default)
        {
            var identity = natsConsumerBuilder.GetRequestReplyIdentity(new RequestReplyIdentity(typeof(TRequest),typeof(TReply))) as NatsRequestReplyIdentity;
            if (identity != null)
            {
                var data = messageSerialization.ToBytes(message);
                var result = await connection.RequestAsync<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>(identity.PublishKey, data, cancellationToken: cancellationToken).ConfigureAwait(false);
                return (TReply)messageSerialization.FromBytes(result.Data, identity.ReplyType);
            }
            throw new InvalidOperationException($"No regist <{typeof(TRequest)},{typeof(TReply)}> handler");
        }

        protected override async Task OnStartAsync(CancellationToken cancellationToken = default)
        {
            await OnStopAsync(cancellationToken).ConfigureAwait(false);

            serviceScope = serviceScopeFactory.CreateScope();
            runningTokenSouce = new CancellationTokenSource();
            await BuildConsumerAsync(runningTokenSouce.Token).ConfigureAwait(false);
            await BuildRequestReplyAsync(runningTokenSouce.Token).ConfigureAwait(false);
        }

        private async Task BuildRequestReplyAsync(CancellationToken token)
        {
            requestReplyIdentities = await natsConsumerBuilder.BuildRequestReplysAsync(token).ConfigureAwait(false);
            requestReplyTasks = new Task[requestReplyIdentities.Count][];
            var index = 0;
            foreach (var item in requestReplyIdentities)
            {
                var serviceType = typeof(IRequestReply<,>).MakeGenericType(item.Key.Request, item.Key.Reply);
                var services = (IRequestReply)serviceScope!.ServiceProvider.GetRequiredService(serviceType);
                var scaleTasks = new Task[item.Value.Identity.Scale];
                for (int i = 0; i < scaleTasks.Length; i++)
                {
                    scaleTasks[i]= item.Value.LoopReceiveAsync(services, runningTokenSouce!.Token);
                }
                requestReplyTasks[index++] = scaleTasks;
            }
        }

        private async Task BuildConsumerAsync(CancellationToken token)
        {
            consumerIdentities = await natsConsumerBuilder.BuildConsumersAsync(token).ConfigureAwait(false);
            consumerTasks = new Task[consumerIdentities.Count][];
            var index = 0;
            foreach (var item in consumerIdentities)
            {
                var serviceType = typeof(IConsumer<>).MakeGenericType(item.Key);
                var services = serviceScope!.ServiceProvider.GetServices(serviceType).Cast<IConsumer>().ToArray();
                var scaleTasks = new Task[item.Value.Identity.Scale];
                for (int i = 0; i < scaleTasks.Length; i++)
                {
                    scaleTasks[i] = item.Value.LoopReceiveAsync(services, runningTokenSouce!.Token);
                }
                consumerTasks[index++] = scaleTasks;
            }
        }

        protected override Task OnStopAsync(CancellationToken cancellationToken = default)
        {
            serviceScope?.Dispose();
            runningTokenSouce?.Cancel();
            consumerTasks = null;
            consumerIdentities = null;
            requestReplyTasks = null;
            requestReplyIdentities = null;
            return Task.CompletedTask;
        }
    }
}
