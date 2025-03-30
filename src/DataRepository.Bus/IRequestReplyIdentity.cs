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
}
