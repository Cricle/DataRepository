using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace DataRepository.Bus.InMemory
{
    internal sealed class InMemoryRequestReplyDispatcher : RequestReplyDispatcherBase<RequestReplyBox>
    {
        public InMemoryRequestReplyDispatcher(InMemoryRequestReplyIdentity identity, ILogger logger) : base(logger)
        {
            Identity = identity;
            Channel = identity.CreateChannel<RequestReplyBox>();
            Logger = logger;
        }

        public override IRequestReplyIdentity Identity { get; }

        public Channel<RequestReplyBox> Channel { get; }

        public ILogger Logger { get; }

        protected override ValueTask HandleExceptionAsync(RequestReplyBox? outbox, Exception exception, CancellationToken token)
        {
            outbox?.SetException(exception);
            return base.HandleExceptionAsync(outbox, exception, token);
        }

        protected override async Task HandleRequestAsync(IRequestReply requestReply, RequestReplyBox outbox, CancellationToken token)
        {
            var reply = await requestReply.RequestAsync(outbox.Requqest, token);
            outbox.SetResult(reply);
        }


        protected override IAsyncEnumerable<RequestReplyBox> ReadAsync(CancellationToken token)
        {
            return Channel.Reader.ReadAllAsync(token);
        }
    }
}
