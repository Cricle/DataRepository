namespace DataRepository.Bus
{
    public readonly record struct RequestReplyPair(Type Request, Type Reply);
}
