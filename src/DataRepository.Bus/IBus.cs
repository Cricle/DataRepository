using System.Diagnostics.CodeAnalysis;

namespace DataRepository.Bus
{
    //https://github.com/zarusz/SlimMessageBus/
    public interface IBus : IDisposable, IAsyncDisposable
    {
        bool IsStarted { get; }

        Task StartAsync(CancellationToken cancellationToken = default);

        Task StopAsync(CancellationToken cancellationToken = default);

        Task PublishAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TMessage>(TMessage message, IDictionary<string, object?>? header = null, CancellationToken cancellationToken = default);

        Task PublishManyAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TMessage>(IEnumerable<TMessage> messages, Func<TMessage, IDictionary<string, object?>?>? headerFactory = null, CancellationToken cancellationToken = default);

        Task<TReply> RequestAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TRequest, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TReply>(TRequest message, IDictionary<string, object?>? header = null, CancellationToken cancellationToken = default);
    }
}
