namespace DataRepository.Bus.Exceptions
{
    public interface IRequestReplyExceptionHandler
    {
        Task HandleAsync(Type requestType, Type replyType, Exception exception, CancellationToken token);
    }
}
