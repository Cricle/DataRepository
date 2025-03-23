namespace DataRepository.Bus
{
    public interface IExceptionHandler
    {
        Task HandleAsync(object request, Exception exception, CancellationToken token = default);
    }
}
