using DataRepository.Bus.Internals;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;

namespace DataRepository.Bus
{
    public abstract class BatchBusBase : BatchBusBase<IConsumerDispatcher, IRequestReplyDispatcher>
    {
        protected BatchBusBase(IServiceScopeFactory serviceScopeFactory, ILogger logger, IDispatcherBuilder dispatcherBuilder) : base(serviceScopeFactory, logger, dispatcherBuilder)
        {
        }
    }

    public abstract partial class BatchBusBase<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TConsumerDispatcher, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TRequestReplyDispatcher> : BusBase
        where TConsumerDispatcher : IConsumerDispatcher
        where TRequestReplyDispatcher : IRequestReplyDispatcher
    {
        protected Task[][]? consumerTasks;
        protected Task[][]? requestReplyTasks;
        protected CancellationTokenSource? tokenSource;
        protected IReadOnlyDictionary<Type, TConsumerDispatcher>? consumerIdentities;
        protected IReadOnlyDictionary<RequestReplyPair, TRequestReplyDispatcher>? requestReplyIdentities;
        protected IServiceScope? serviceScope;

        protected BatchBusBase(IServiceScopeFactory serviceScopeFactory, ILogger logger, IDispatcherBuilder dispatcherBuilder)
        {
            ServiceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            DispatcherBuilder = dispatcherBuilder ?? throw new ArgumentNullException(nameof(dispatcherBuilder));
        }

        public ILogger Logger { get; }

        public IDispatcherBuilder DispatcherBuilder { get; }

        public IServiceScopeFactory ServiceScopeFactory { get; }

        public override async Task PublishAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TMessage>(TMessage message, IDictionary<string, object?>? header = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(message);
            var messageType = typeof(TMessage);
            using (var activity = BusActivities.busSource.StartActivity("Publish", ActivityKind.Internal, default(ActivityContext), [new KeyValuePair<string, object?>("messageType", messageType.FullName)]))
            {
                var identity = DispatcherBuilder.GetConsumerIdentity(messageType) ?? throw new InvalidOperationException($"No {messageType} handler registed");
                try
                {
                    if (activity != null && header != null && header.Count != 0)
                    {
                        foreach (var item in header)
                        {
                            activity.AddTag(item.Key, item.Value);
                        }
                    }

                    await CorePublishAsync(message!, identity, header, cancellationToken);
                    busMeter.IncrPublish(true, messageType.FullName!);
                }
                catch (Exception ex)
                {
                    activity?.AddException(ex);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    busMeter.IncrPublish(false, messageType.FullName!);
                    throw;
                }
            }
        }

        protected internal abstract Task CorePublishAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TMessage>(TMessage message, IConsumerIdentity identity, IDictionary<string, object?>? header = null, CancellationToken cancellationToken = default);

        public override async Task<TReply> RequestAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TRequest, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TReply>(TRequest message, IDictionary<string, object?>? header = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(message);

            var identity = new RequestReplyPair(typeof(TRequest), typeof(TReply));
            using (var activity = BusActivities.busSource.StartActivity("Request", ActivityKind.Internal, default(ActivityContext),
                [new KeyValuePair<string, object?>("requestType", identity.Request.FullName), new KeyValuePair<string, object?>("replyType", identity.Reply.FullName)]))
            {
                var rrIdentity = DispatcherBuilder.GetRequestReplyIdentity(identity) ?? throw new InvalidOperationException($"No <{typeof(TRequest)},{typeof(TReply)}> handler registed");
                try
                {
                    if (activity != null && header != null && header.Count != 0)
                    {
                        foreach (var item in header)
                        {
                            activity.AddTag(item.Key, item.Value);
                        }
                    }

                    var res = await CoreRequestAsync<TRequest, TReply>(message, rrIdentity, header, cancellationToken);
                    busMeter.IncrRequest(true, identity.Request.FullName!, identity.Reply.FullName!);
                    return res;
                }
                catch (Exception ex)
                {
                    activity?.AddException(ex);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

                    busMeter.IncrRequest(false, identity.Request.FullName!, identity.Reply.FullName!);
                    throw;
                }
            }
        }

        protected internal abstract Task<TReply> CoreRequestAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TRequest, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TReply>(TRequest message, IRequestReplyIdentity identity, IDictionary<string, object?>? header = null, CancellationToken cancellationToken = default);

        protected internal override async Task OnStartAsync(CancellationToken cancellationToken = default)
        {
            await OnStopAsync(cancellationToken);

            serviceScope = ServiceScopeFactory.CreateScope();
            tokenSource = new CancellationTokenSource();

            await BuildConsumerAsync(tokenSource.Token);
            await BuildRequestReplyAsync(tokenSource.Token);
        }

        protected internal override Task OnStopAsync(CancellationToken cancellationToken = default)
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

        [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
        internal async Task BuildConsumerAsync(CancellationToken token)
        {
            var consumers = await DispatcherBuilder.BuildConsumersAsync(token).ConfigureAwait(false);
            consumerIdentities = consumers.ToDictionary(x => x.Key, x => (TConsumerDispatcher)x.Value);
            consumerTasks = new Task[consumers.Count][];
            var index = 0;
            foreach (var item in consumerIdentities)
            {
                var serviceType = typeof(IBatchConsumer<>).MakeGenericType(item.Key);
                var services = serviceScope!.ServiceProvider.GetServices(serviceType).Cast<IBatchConsumer>().ToArray();
                var batch = new Task[item.Value.Identity.Scale];
                for (int i = 0; i < batch.Length; i++)
                {
                    batch[i] = item.Value.LoopReceiveAsync(services, tokenSource!.Token).ContinueWith(t =>
                    {
                        HandleConsumerWorkerComplated(item.Key, item.Value, t);
                    }, token);
                }
                consumerTasks[index++] = batch;
            }
        }

        [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
        internal async Task BuildRequestReplyAsync(CancellationToken token)
        {
            var requestReplies = await DispatcherBuilder.BuildRequestReplysAsync(token).ConfigureAwait(false);
            requestReplyIdentities = requestReplies.ToDictionary(x => x.Key, x => (TRequestReplyDispatcher)x.Value);
            requestReplyTasks = new Task[requestReplyIdentities.Count][];
            var index = 0;
            foreach (var item in requestReplyIdentities)
            {
                var serviceType = typeof(IRequestReply<,>).MakeGenericType(item.Key.Request, item.Key.Reply);
                var services = (IRequestReply)serviceScope!.ServiceProvider.GetRequiredService(serviceType);
                var batch = new Task[item.Value.Identity.Scale];
                for (int i = 0; i < batch.Length; i++)
                {
                    batch[i] = item.Value.LoopReceiveAsync(services, tokenSource!.Token).ContinueWith(t =>
                    {
                        HandleRequestReplyWorkerComplated(item.Key, item.Value, t);
                    }, token);
                }
                requestReplyTasks[index++] = batch;
            }
        }

        internal void HandleRequestReplyWorkerComplated(RequestReplyPair identity, IRequestReplyDispatcher dispatcher, Task task)
        {
            if (task.IsFaulted)
            {
                RequestReplyFaultedError(Logger, task.Exception, identity.Request, identity.Reply);
            }
            else
            {
                RequestReplyComplatedInfo(Logger, identity.Request, identity.Reply);
            }
            OnHandleRequestReplyWorkerComplated(identity, dispatcher, task);
        }

        internal void HandleConsumerWorkerComplated(Type type, IConsumerDispatcher dispatcher, Task task)
        {
            if (task.IsFaulted)
            {
                ConsumerFaultedError(Logger, task.Exception, type);
            }
            else
            {
                ConsumerComplatedInfo(Logger, type);
            }
            OnHandleConsumerWorkerComplated(type, dispatcher, task);
        }

        protected internal virtual void OnHandleConsumerWorkerComplated(Type type, IConsumerDispatcher dispatcher, Task task) { }

        protected internal virtual void OnHandleRequestReplyWorkerComplated(RequestReplyPair identity, IRequestReplyDispatcher dispatcher, Task task) { }

        [LoggerMessage(Message = "The request-reply <{request},{reply}> task was exit", Level = LogLevel.Error)]
        internal static partial void RequestReplyFaultedError(ILogger logger, Exception ex, Type request, Type reply);

        [LoggerMessage(Message = "The request-reply <{request},{reply}> task was exit", Level = LogLevel.Information)]
        internal static partial void RequestReplyComplatedInfo(ILogger logger, Type request, Type reply);

        [LoggerMessage(Message = "The consumer <{consumer}> task was exit", Level = LogLevel.Error)]
        internal static partial void ConsumerFaultedError(ILogger logger, Exception ex, Type consumer);

        [LoggerMessage(Message = "The consumer <{consumer}> task was exit", Level = LogLevel.Information)]
        internal static partial void ConsumerComplatedInfo(ILogger logger, Type consumer);
    }
    public abstract class BusBase : IBus
    {
        internal readonly BusMeter busMeter;

        protected Meter Meter => busMeter.meter;

        private int isStarted = 0;

        public bool IsStarted => Volatile.Read(ref isStarted) == 1;

        protected BusBase()
        {
            var type = GetType();
            busMeter = new BusMeter(type.FullName!, BusActivities.Version);
        }

        public void Dispose()
        {
            StopAsync().GetAwaiter().GetResult();
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await StopAsync();
            GC.SuppressFinalize(this);
        }

        public abstract Task PublishAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TMessage>(TMessage message, IDictionary<string, object?>? header = null, CancellationToken cancellationToken = default);

        public virtual async Task PublishManyAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TMessage>(IEnumerable<TMessage> messages, Func<TMessage, IDictionary<string, object?>?>? headerFactory = null, CancellationToken cancellationToken = default)
        {
            foreach (var item in messages)
            {
                await PublishAsync(item, headerFactory?.Invoke(item), cancellationToken);
            }
        }

        public abstract Task<TReply> RequestAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TRequest, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TReply>(TRequest message, IDictionary<string, object?>? header = null, CancellationToken cancellationToken = default);

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (Interlocked.CompareExchange(ref isStarted, 1, 0) == 0)
                await OnStartAsync(cancellationToken);
        }

        protected internal abstract Task OnStartAsync(CancellationToken cancellationToken = default);

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (Interlocked.CompareExchange(ref isStarted, 0, 1) == 1)
                await OnStopAsync(cancellationToken);
        }

        protected internal abstract Task OnStopAsync(CancellationToken cancellationToken = default);
    }
}
