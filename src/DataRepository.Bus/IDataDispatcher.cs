using System.Diagnostics.CodeAnalysis;

namespace DataRepository.Bus
{
    public interface IDataDispatcher<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TIdentity, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TContext>
    {
        TIdentity Identity { get; }

        Task LoopReceiveAsync(TContext context, CancellationToken token = default);
    }
}
