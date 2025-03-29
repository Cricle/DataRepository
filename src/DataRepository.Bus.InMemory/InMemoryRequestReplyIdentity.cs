using System.Threading.Channels;

namespace DataRepository.Bus.InMemory
{
    public class InMemoryRequestReplyIdentity :ChannelIdentity, IRequestReplyIdentity
    {
        public InMemoryRequestReplyIdentity(Type requestType, Type replyType, bool concurrentHandle, bool unBoundChannel, UnboundedChannelOptions? unBoundedChannelOptions, BoundedChannelOptions? boundedChannelOptions, uint scale)
            : base(unBoundChannel, unBoundedChannelOptions, boundedChannelOptions)
        {
            RequestType = requestType;
            ReplyType = replyType;
            ConcurrentHandle = concurrentHandle;
            Scale = scale;
        }

        public Type RequestType { get; }

        public Type ReplyType { get; }

        public uint Scale { get; }

        public bool ConcurrentHandle { get; }
    }
}
