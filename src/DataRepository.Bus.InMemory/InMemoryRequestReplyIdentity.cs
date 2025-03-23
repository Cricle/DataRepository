using System.Threading.Channels;

namespace DataRepository.Bus.InMemory
{
    public class InMemoryRequestReplyIdentity :ChannelIdentity, IRequestReplyIdentity
    {
        public InMemoryRequestReplyIdentity(Type requestType, Type replyType, bool concurrentHandle, bool unBoundChannel, UnboundedChannelOptions? unBoundedChannelOptions, BoundedChannelOptions? boundedChannelOptions)
            : base(unBoundChannel, unBoundedChannelOptions, boundedChannelOptions)
        {
            RequestType = requestType;
            ReplyType = replyType;
            ConcurrentHandle = concurrentHandle;
        }

        public Type RequestType { get; }

        public Type ReplyType { get; }

        uint IRequestReplyIdentity.Scale { get; } = 1;

        public bool ConcurrentHandle { get; }
    }
}
