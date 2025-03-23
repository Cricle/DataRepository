using NATS.Client.Core;

namespace DataRepository.Bus.Nats
{
    public class NatsRequestReplyIdentity : IRequestReplyIdentity
    {
        public NatsRequestReplyIdentity(Type requestType, Type replyType, string subject, string publishKey, string? queueGroup, NatsSubOpts? natsSubOpts, uint scale)
        {
            RequestType = requestType;
            ReplyType = replyType;
            Subject = subject;
            QueueGroup = queueGroup;
            NatsSubOpts = natsSubOpts;
            PublishKey = publishKey;
            Scale = scale;
        }

        public Type RequestType { get; }

        public Type ReplyType { get; }

        public string Subject { get; }

        public string PublishKey { get; }

        public string? QueueGroup { get; }

        public NatsSubOpts? NatsSubOpts { get; }

        public uint Scale { get; }
    }
}
