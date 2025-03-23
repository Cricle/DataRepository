using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace DataRepository.Bus.InMemory
{
    internal sealed class InMemoryConsumerDispatcher : IConsumerDispatcher
    {
        public InMemoryConsumerDispatcher(InMemoryConsumerIdentity identity, ILogger logger)
        {
            Identity = identity;
            Channel = identity.CreateChannel<object>();
            Logger = logger;
        }

        public InMemoryConsumerIdentity Identity { get; }

        public Channel<object> Channel { get; }

        public ILogger Logger { get; }

        IConsumerIdentity IDataDispatcher<IConsumerIdentity, IReadOnlyList<IConsumer>>.Identity => Identity;

        public async Task LoopReceiveAsync(IReadOnlyList<IConsumer> context, CancellationToken token = default)
        {
            var tasks = new Task[context.Count];
            var identity = Identity;
            var reader = Channel.Reader;
            while (!token.IsCancellationRequested)
            {
                var res = await reader.ReadAsync(token);
                try
                {
                    if (identity.ParallelConsumer)
                    {
                        for (int i = 0; i < context.Count; i++)
                        {
                            tasks[i] = context[i].HandleAsync(identity, token);
                        }

                        if (!identity.ConcurrentHandle)
                        {
                            await Task.WhenAll(tasks);
                        }
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
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Logger.LogError(ex, "When handle Consumer {type} error", identity.MessageType);
                }
            }
        }
    }
}
