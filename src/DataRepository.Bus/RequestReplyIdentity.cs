namespace DataRepository.Bus
{
    public record struct RequestReplyIdentity(Type Request, Type Reply);
}
