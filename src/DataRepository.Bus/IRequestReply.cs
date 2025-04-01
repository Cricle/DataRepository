namespace DataRepository.Bus
{
    public interface IRequestReply
    {
        Task<object> RequestAsync(object request, CancellationToken token = default);
    }

    public interface IRequestReply<TRequest, TReply> : IRequestReply
    {
        Task<TReply> RequestAsync(TRequest request, CancellationToken token = default);

        async Task<object> IRequestReply.RequestAsync(object request, CancellationToken token)
            => (await RequestAsync((TRequest)request, token).ConfigureAwait(false))!;
    }
}
