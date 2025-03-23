using Microsoft.Extensions.Logging;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DataRepository.Bus.InMemory
{
    internal sealed class InMemoryRequestReplyDispatcher : IRequestReplyDispatcher
    {
        public InMemoryRequestReplyDispatcher(InMemoryRequestReplyIdentity identity,ILogger logger)
        {
            Identity = identity;
            Channel = identity.CreateChannel<RequestReplyBox>();
            Logger = logger;
        }

        public InMemoryRequestReplyIdentity Identity { get; }

        public Channel<RequestReplyBox> Channel{ get; }

        public ILogger Logger { get; }

        IRequestReplyIdentity IDataDispatcher<IRequestReplyIdentity, IRequestReply>.Identity => Identity;

        public async Task LoopReceiveAsync(IRequestReply context, CancellationToken token = default)
        {
            var identity = Identity;
            var reader = Channel.Reader;
            while (!token.IsCancellationRequested)
            {
                var res = await reader.ReadAsync(token);
                try
                {
                    if (identity.ConcurrentHandle)
                    {
                        _ = Task.Factory.StartNew(async () =>
                        {
                            try
                            {
                                var reply = await context.RequestAsync(res.Requqest, token);
                                res.ReplySource.SetResult(reply);
                            }
                            catch (Exception ex)
                            {
                                Logger.LogError(ex, "When handle Request-Reply <{request},{reply}> error", identity.RequestType, identity.ReplyType);
                            }
                        }).ConfigureAwait(false);
                    }
                    else
                    {
                        var reply = await context.RequestAsync(res.Requqest, token).ConfigureAwait(false);
                        res.ReplySource.SetResult(reply);
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Logger.LogError(ex, "When handle Request-Reply <{request},{reply}> error", identity.RequestType, identity.ReplyType);
                    res.ReplySource.SetException(ex);
                }
            }
        }
    }
}
