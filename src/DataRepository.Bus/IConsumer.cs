using System.Diagnostics.CodeAnalysis;

namespace DataRepository.Bus
{
    public interface IConsumer
    {
        Task HandleAsync(object data, CancellationToken cancellationToken = default);
    }

    public interface IConsumer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TMessage> : IConsumer
    {
        Task HandleAsync(TMessage message, CancellationToken cancellationToken = default);

        Task IConsumer.HandleAsync(object data, CancellationToken cancellationToken)
             => HandleAsync((TMessage)data, cancellationToken);
    }
}
