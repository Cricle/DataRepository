using DataRepository.Bus.Buffer;
using DataRepository.Bus.Internals;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

namespace DataRepository.Bus
{
    public abstract class ConsumerDispatcherBase<TContext> : IConsumerDispatcher
    {
        internal readonly ConsumerDispatcherMeter cdMeter;
        protected Meter Meter => cdMeter.meter;

        protected static readonly Task<TContext?> nullContext = Task.FromResult<TContext?>(default);

        protected readonly ILogger logger;

        protected ConsumerDispatcherBase(ILogger logger)
        {
            this.logger = logger;
            var type = GetType();
            cdMeter = new ConsumerDispatcherMeter(type.FullName!, BusActivities.Version);
            cdMeter.pendingMessageFunc = GetPenddingMessageCount;
        }

        public abstract IConsumerIdentity Identity { get; }

        protected virtual async IAsyncEnumerable<object?> ReadAsync(TContext? context, [EnumeratorCancellation] CancellationToken token)
        {
            yield return await ReadSingleAsync(context, token);
        }

        protected virtual Task<TContext?> StartingCreateContextAsync(IReadOnlyList<IConsumer> context, CancellationToken token = default) => nullContext;

        protected abstract Task<object?> ReadSingleAsync(TContext? context, CancellationToken token);

        protected virtual ValueTask HandleErrorAsync(TContext? context, Exception exception, CancellationToken token)
        {
            if (!token.IsCancellationRequested)
                logger.LogError(exception, "When handle Consumer {type} error", Identity.MessageType);
            return ValueTask.CompletedTask;
        }

        protected virtual long GetPenddingMessageCount() => 0;

        private async Task SingleLoopReceiveAsync(IReadOnlyList<IConsumer> context, CancellationToken token = default)
        {
            var tasks = new Task[context.Count];
            var identity = Identity;
            object? res = null;
            var messageType = identity.MessageType.FullName;
            var startContext = await StartingCreateContextAsync(context, token);
            try
            {
                while (!token.IsCancellationRequested)
                {
                    using (var activity = BusActivities.consumerSource.StartActivity("Consumer", ActivityKind.Internal, default(ActivityContext), [new KeyValuePair<string, object?>("messageType", messageType)]))
                    {
                        activity?.SetTag("batch", false);
                        try
                        {

                            res = await ReadSingleAsync(startContext, token);
                            if (res == null)
                            {
                                activity?.AddEvent(new ActivityEvent("EmptyMessage"));
                                continue;
                            }

                            if (identity.ParallelConsumer && context.Count > 1)
                            {
                                for (int i = 0; i < context.Count; i++)
                                {
                                    tasks[i] = context[i].HandleAsync(identity, token);
                                }

                                await Task.WhenAll(tasks);
                            }
                            else
                            {
                                for (int i = 0; i < context.Count; i++)
                                {
                                    var tsk = context[i].HandleAsync(res, token);
                                    if (!identity.ConcurrentHandle)
                                    {
                                        await tsk;
                                    }
                                }
                            }
                            cdMeter.AddHandle(1, true, messageType!);
                        }
                        catch (Exception ex)
                        {
                            activity?.AddException(ex);
                            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

                            cdMeter.AddHandle(1, false, messageType!);
                            await HandleErrorAsync(startContext, ex, token);
                        }
                    }
                }
            }
            finally
            {
                if (startContext is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        private async Task BatchLoopReceiveAsync(IReadOnlyList<IBatchConsumer> context, CancellationToken token = default)
        {
            var tasks = new Task[context.Count];
            var identity = Identity;
            var buffer = new MessageBuffer<object>(identity.BatchSize);
            var hasWaitTime = identity.FetchTime != null;
            long locker = 0;
            PeriodicTimer? timer = null;
            var messageType = identity.MessageType.FullName;
            var startContext = await StartingCreateContextAsync(context, token);
            async Task HandleConsumersAsync()
            {
                using (var activity = BusActivities.consumerSource.StartActivity("Consumer", ActivityKind.Internal, default(ActivityContext), [new KeyValuePair<string, object?>("messageType", messageType)]))
                {
                    var msg = buffer.ToBathMessages();
                    activity?.SetTag("batch", true);
                    activity?.SetTag("batch_size", msg.Size);
                    try
                    {
                        if (Identity.ParallelConsumer && context.Count > 1)
                        {
                            for (int i = 0; i < context.Count; i++)
                            {
                                tasks[i] = context[i].HandleAsync(msg, token);
                            }
                            await Task.WhenAll(tasks).ConfigureAwait(false);
                        }
                        else
                        {
                            for (int i = 0; i < context.Count; i++)
                            {
                                var tsk = context[i].HandleAsync(msg, token).ConfigureAwait(false);
                                if (!Identity.ConcurrentHandle)
                                {
                                    await tsk;
                                }
                            }
                        }
                        cdMeter.AddHandle(buffer.Index, false, messageType!);
                        buffer.Clear();
                    }
                    catch (Exception ex)
                    {
                        activity?.AddException(ex);
                        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

                        cdMeter.AddHandle(buffer.Index, true, messageType!);
                        await HandleErrorAsync(startContext, ex, token);
                    }
                }
            }

            if (hasWaitTime)
            {
                timer = new PeriodicTimer(identity.FetchTime!.Value);
                _ = Task.Factory.StartNew(async () =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        var ok = await timer.WaitForNextTickAsync(token).ConfigureAwait(false);
                        if (ok && buffer.Index != 0 && Interlocked.CompareExchange(ref locker, 1, 0) == 0)
                        {
                            await HandleConsumersAsync().ConfigureAwait(false);
                            Interlocked.Exchange(ref locker, 0);
                        }
                    }
                }, TaskCreationOptions.LongRunning);
            }
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await foreach (var item in ReadAsync(startContext, token).ConfigureAwait(false))
                    {
                        if (item == null) continue;
                        if (!buffer.Add(item)) continue;
                        if (Interlocked.CompareExchange(ref locker, 1, 0) == 0)
                        {
                            await HandleConsumersAsync().ConfigureAwait(false);
                            Interlocked.Exchange(ref locker, 0);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await HandleErrorAsync(startContext, ex, token);
                }
            }
        }

        public Task LoopReceiveAsync(IReadOnlyList<IBatchConsumer> context, CancellationToken token = default)
        {
            if (Identity.BatchSize == 1)
                return SingleLoopReceiveAsync(context, token);
            return BatchLoopReceiveAsync(context, token);
        }
    }

}
