﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace DataRepository.Bus
{
    public abstract class BatchBusBase : BatchBusBase<IConsumerDispatcher, IRequestReplyDispatcher>
    {
        protected BatchBusBase(IServiceScopeFactory serviceScopeFactory, ILogger logger, IDispatcherBuilder dispatcherBuilder) : base(serviceScopeFactory, logger, dispatcherBuilder)
        {
        }
    }

    public abstract partial class BatchBusBase<TConsumerDispatcher,TRequestReplyDispatcher> : BusBase
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
            ServiceScopeFactory = serviceScopeFactory;
            Logger = logger;
            DispatcherBuilder = dispatcherBuilder;
        }

        public ILogger Logger { get; }

        public IDispatcherBuilder DispatcherBuilder { get; }

        public IServiceScopeFactory ServiceScopeFactory { get; }

        public override async Task PublishAsync<TMessage>(TMessage message, IDictionary<string, object?>? header = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(message);
            var identity = DispatcherBuilder.GetConsumerIdentity(typeof(TMessage));
            if (identity == null)
            {
                throw new InvalidOperationException($"No {typeof(TMessage)} handler registed");
            }
            await CorePublishAsync(message!,identity, header, cancellationToken);
        }
        
        protected abstract Task CorePublishAsync<TMessage>(TMessage message,IConsumerIdentity identity, IDictionary<string, object?>? header = null, CancellationToken cancellationToken = default);

        public override async Task<TReply> RequestAsync<TRequest, TReply>(TRequest message, IDictionary<string, object?>? header = null, CancellationToken cancellationToken = default)
        {
           ArgumentNullException.ThrowIfNull(message);

            var identity = new RequestReplyPair(typeof(TRequest), typeof(TReply));
            var rrIdentity = DispatcherBuilder.GetRequestReplyIdentity(identity);
            if (rrIdentity == null)
            {
                throw new InvalidOperationException($"No <{typeof(TRequest)},{typeof(TReply)}> handler registed");
            }
            return await CoreRequestAsync<TRequest, TReply>(message, rrIdentity, header, cancellationToken);
        }

        protected abstract Task<TReply> CoreRequestAsync<TRequest, TReply>(TRequest message, IRequestReplyIdentity identity, IDictionary<string, object?>? header = null, CancellationToken cancellationToken = default);
       
        protected override async Task OnStartAsync(CancellationToken cancellationToken = default)
        {
            await OnStopAsync(cancellationToken);

            serviceScope = ServiceScopeFactory.CreateScope();
            tokenSource = new CancellationTokenSource();

            await BuildConsumerAsync(tokenSource.Token);
            await BuildRequestReplyAsync(tokenSource.Token);
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

        [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
        private async Task BuildConsumerAsync(CancellationToken token)
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
        private async Task BuildRequestReplyAsync(CancellationToken token)
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

        private void HandleRequestReplyWorkerComplated(RequestReplyPair identity, IRequestReplyDispatcher dispatcher, Task task)
        {
            if (task.IsFaulted)
            {
                RequestReplyFaultedError(Logger, task.Exception,identity.Request,identity.Reply);
            }
            else
            {
                RequestReplyComplatedInfo(Logger, identity.Request, identity.Reply);
            }
            OnHandleRequestReplyWorkerComplated(identity, dispatcher, task);
        }

        private void HandleConsumerWorkerComplated(Type type, IConsumerDispatcher dispatcher, Task task)
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

        protected virtual void OnHandleConsumerWorkerComplated(Type type, IConsumerDispatcher dispatcher, Task task) { }

        protected virtual void OnHandleRequestReplyWorkerComplated(RequestReplyPair identity, IRequestReplyDispatcher dispatcher, Task task) { }

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
        private int isStarted = 0;

        public bool IsStarted => Volatile.Read(ref isStarted) == 1;

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

        public abstract Task PublishAsync<TMessage>(TMessage message, IDictionary<string, object?>? header = null, CancellationToken cancellationToken = default);

        public virtual async Task PublishManyAsync<TMessage>(IEnumerable<TMessage> messages, Func<TMessage, IDictionary<string, object?>?>? headerFactory = null, CancellationToken cancellationToken = default)
        {
            foreach (var item in messages)
            {
                await PublishAsync(item, headerFactory?.Invoke(item), cancellationToken);
            }
        }

        public abstract Task<TReply> RequestAsync<TRequest, TReply>(TRequest message, IDictionary<string, object?>? header = null, CancellationToken cancellationToken = default);

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (Interlocked.CompareExchange(ref isStarted, 1, 0) == 0)
                await OnStartAsync(cancellationToken);
        }

        protected abstract Task OnStartAsync(CancellationToken cancellationToken = default);

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (Interlocked.CompareExchange(ref isStarted, 0, 1) == 1)
                await OnStopAsync(cancellationToken);
        }

        protected abstract Task OnStopAsync(CancellationToken cancellationToken = default);
    }
}
