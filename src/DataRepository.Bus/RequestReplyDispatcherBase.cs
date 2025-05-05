using DataRepository.Bus.Internals;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace DataRepository.Bus
{
    public abstract class RequestReplyDispatcherBase<TOutbox> : IRequestReplyDispatcher
    {
        internal readonly RequestReplyDispatcherMeter rrMeter;
        protected internal Meter Meter => rrMeter.meter;

        protected internal readonly ILogger logger;

        protected RequestReplyDispatcherBase(ILogger logger)
        {
            this.logger = logger;
            var type = GetType();
            rrMeter = new RequestReplyDispatcherMeter(type.FullName!, BusActivities.Version);
            rrMeter.pendingMessageFunc = GetPenddingMessageCount;
        }

        public abstract IRequestReplyIdentity Identity { get; }

        protected internal abstract IAsyncEnumerable<TOutbox> ReadAsync(CancellationToken token);

        protected internal virtual ValueTask HandleExceptionAsync(TOutbox? outbox, Exception exception, CancellationToken token)
        {
            if (!token.IsCancellationRequested)
            {
                logger.LogError(exception, "When handle Request-Reply <{request},{reply}> error", Identity.RequestType, Identity.ReplyType);
            }
            return ValueTask.CompletedTask;
        }

        protected internal abstract Task HandleRequestAsync(IRequestReply requestReply, TOutbox outbox, CancellationToken token);

        protected internal virtual long GetPenddingMessageCount() => 0;

        public async Task LoopReceiveAsync(IRequestReply context, CancellationToken token = default)
        {
            var identity = Identity;
            var rquestType = identity.RequestType.FullName!;
            var replyType = identity.ReplyType.FullName!;
            try
            {
                await foreach (var res in ReadAsync(token).ConfigureAwait(false))
                {
                    var activity = BusActivities.consumerSource.StartActivity("Consumer", ActivityKind.Internal, default(ActivityContext),
                        [new KeyValuePair<string, object?>("rquestType", rquestType), new KeyValuePair<string, object?>("replyType", replyType)]);
                    activity?.SetTag("concurrent_handle", identity.ConcurrentHandle);
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
                                finally
                                {
                                    activity?.Dispose();
                                }
                            }).ConfigureAwait(false);
                        }
                        else
                        {
                            await HandleRequestAsync(context, res, token).ConfigureAwait(false);
                        }
                        rrMeter.AddHandle(true, rquestType, replyType);
                    }
                    catch (Exception ex)
                    {
                        activity?.AddException(ex);
                        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

                        rrMeter.AddHandle(false, rquestType, replyType);
                        await HandleExceptionAsync(res, ex, token).ConfigureAwait(false);
                    }
                    finally 
                    {
                        if (activity != null && !identity.ConcurrentHandle)
                        {
                            activity.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(default, ex, token).ConfigureAwait(false);
            }
        }
    }
}
