namespace DataRepository.Bus.Exceptions
{
    public interface IConsumerExceptionHandler
    {
        Task HandleAsync(Type messageType, Exception exception, CancellationToken token);
    }
}
