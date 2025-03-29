using Microsoft.Extensions.Logging;

namespace DataRepository.Bus
{
    public interface IRequestReplyIdentity
    {
        Type RequestType { get; }

        Type ReplyType { get; }

        uint Scale { get; }

        bool ConcurrentHandle { get; }

        RequestReplyPair RequestReplyPair => new RequestReplyPair(RequestType, ReplyType);
    }

    public interface IRequestReplyDispatcher : IDataDispatcher<IRequestReplyIdentity, IRequestReply>
    {
    }
    public abstract class RequestReplyDispatcherBase<TOutbox> : IRequestReplyDispatcher
    {
        protected readonly ILogger logger;

        protected RequestReplyDispatcherBase(ILogger logger)
        {
            this.logger = logger;
        }

        public abstract IRequestReplyIdentity Identity { get; }

        protected abstract IAsyncEnumerable<TOutbox> ReadAsync(CancellationToken token);

        protected virtual ValueTask HandleExceptionAsync(TOutbox? outbox, Exception exception, CancellationToken token)
        {
            if (!token.IsCancellationRequested)
            {
                logger.LogError(exception, "When handle Request-Reply <{request},{reply}> error", Identity.RequestType, Identity.ReplyType);
            }
            return ValueTask.CompletedTask;
        }

        protected abstract Task HandleRequestAsync(IRequestReply requestReply, TOutbox outbox, CancellationToken token);

        public async Task LoopReceiveAsync(IRequestReply context, CancellationToken token = default)
        {
            var identity = Identity;
            try
            {
                await foreach (var res in ReadAsync(token).ConfigureAwait(false))
                {
                    try
                    {
                        if (identity.ConcurrentHandle)
                        {
                            var catched = res;
                            _ = Task.Factory.StartNew(async () =>
                            {
                                try
                                {
                                    await HandleRequestAsync(context, catched, token).ConfigureAwait(false);
                                }
                                catch (Exception ex)
                                {
                                    await HandleExceptionAsync(catched, ex, token).ConfigureAwait(false);
                                }
                            }).ConfigureAwait(false);
                        }
                        else
                        {
                            await HandleRequestAsync(context, res, token).ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        await HandleExceptionAsync(res, ex, token).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(default, ex, token).ConfigureAwait(false);
            }
        }
    }

    public interface IRequestReply
    {
        Task<object> RequestAsync(object request, CancellationToken token = default);
    }

    public interface IRequestReply<TRequest, TReply> : IRequestReply
    {
        Task<TReply> RequestAsync(TRequest request, CancellationToken token = default);

        async Task<object> IRequestReply.RequestAsync(object request, CancellationToken token)
            => (await RequestAsync((TRequest)request, token).ConfigureAwait(false))!;
    }
}
