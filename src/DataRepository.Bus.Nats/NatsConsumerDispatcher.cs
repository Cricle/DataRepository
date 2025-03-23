using DataRepository.Bus.Serialization;
using Microsoft.Extensions.Logging;
using NATS.Client.JetStream;

namespace DataRepository.Bus.Nats
{
    public class NatsConsumerDispatcher : IConsumerDispatcher
    {
        public NatsConsumerDispatcher(NatsConsumerIdentity identity, INatsJSStream stream, ILogger logger, IMessageSerialization messageSerialization)
        {
            Identity = identity;
            Stream = stream;
            Logger = logger;
            MessageSerialization = messageSerialization;
        }

        public INatsJSStream Stream { get; }

        public NatsConsumerIdentity Identity { get; }

        public ILogger Logger { get; }

        public IMessageSerialization MessageSerialization { get; }

        IConsumerIdentity IDataDispatcher<IConsumerIdentity, IReadOnlyList<IConsumer>>.Identity => Identity;

        public async Task LoopReceiveAsync(IReadOnlyList<IConsumer> consumers, CancellationToken token = default)
        {
            var messageSerialization = MessageSerialization;
            var logger = Logger;
            var identity = Identity;
            try
            {
                var jsConsumer = await Stream.CreateOrUpdateConsumerAsync(identity.ConsumerConfig, token).ConfigureAwait(false);
                NatsJSMsg<ReadOnlyMemory<byte>>? msg = null;
                var parallelTasks = new Task[consumers.Count];
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        msg = await jsConsumer.NextAsync<ReadOnlyMemory<byte>>(null, identity.NatsJSNextOpts, token).ConfigureAwait(false);
                        if (!msg.HasValue) continue;

                        if (identity.ParallelConsumer)
                        {
                            for (int i = 0; i < consumers.Count; i++)
                                parallelTasks[i] = consumers[i].HandleAsync(messageSerialization.FromBytes(msg.Value.Data, identity.MessageType), token);
                            await Task.WhenAll(parallelTasks).ConfigureAwait(false);
                        }
                        else
                        {
                            for (int i = 0; i < consumers.Count; i++)
                                await consumers[i].HandleAsync(msg.Value.Data, token).ConfigureAwait(false);
                        }

                        await msg.Value.AckAsync(default, token);
                    }
                    catch (Exception ex) when (!token.IsCancellationRequested)
                    {
                        logger.LogError(ex, "{Subject} handler error", identity.Subject);
                        if (msg!=null)
                        {
                            await msg.Value.NakAsync(cancellationToken: token);
                        }
                    }
                    finally
                    {
                        msg = null;
                        if (identity.ParallelConsumer)
                        {
                            Array.Clear(parallelTasks, 0, parallelTasks.Length);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{Subject} Init fail", identity.Subject);
            }
        }
    }
}
