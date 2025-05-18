using System.Diagnostics.CodeAnalysis;

namespace DataRepository.Bus
{
    public interface IRequestReply
    {
        Task<object> RequestAsync(object request, CancellationToken token = default);
    }

    public interface IRequestReply<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TRequest, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TReply> : IRequestReply
    {
        Task<TReply> RequestAsync(TRequest request, CancellationToken token = default);

        async Task<object> IRequestReply.RequestAsync(object request, CancellationToken token)
            => (await RequestAsync((TRequest)request, token).ConfigureAwait(false))!;
    }
}
