using DataRepository.Bus.Buffer;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace DataRepository.Bus
{
    public interface IConsumerDispatcher : IDataDispatcher<IConsumerIdentity, IReadOnlyList<IBatchConsumer>>
    {
    }
    public abstract class ConsumerDispatcherBase<TContext> : IConsumerDispatcher
    {
        protected static readonly Task<TContext?> nullContext = Task.FromResult<TContext?>(default);

        protected readonly ILogger logger;

        protected ConsumerDispatcherBase(ILogger logger)
        {
            this.logger = logger;
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

        private async Task SingleLoopReceiveAsync(IReadOnlyList<IConsumer> context, CancellationToken token = default)
        {
            var tasks = new Task[context.Count];
            var identity = Identity;
            object? res = null;
            var startContext = await StartingCreateContextAsync(context, token);
            try
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        res = await ReadSingleAsync(startContext, token);
                        if (res == null) continue;

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
                    }
                    catch (Exception ex)
                    {
                        await HandleErrorAsync(startContext, ex, token);
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
            var startContext = await StartingCreateContextAsync(context, token);
            async Task HandleConsumersAsync()
            {
                try
                {
                    var msg = buffer.ToBathMessages();
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
                }
                catch (Exception ex)
                {
                    await HandleErrorAsync(startContext, ex, token);
                }
                finally
                {
                    buffer.Clear();
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
