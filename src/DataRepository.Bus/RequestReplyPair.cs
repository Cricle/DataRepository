namespace DataRepository.Bus
{
    public record struct RequestReplyPair(Type Request, Type Reply);
}
