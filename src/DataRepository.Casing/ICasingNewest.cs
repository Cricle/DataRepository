using DataRepository.Casing.Models;
#if !NETSTANDARD
using System.Diagnostics.CodeAnalysis;
#endif

namespace DataRepository.Casing
{
    public interface ICasingNewest<
#if !NETSTANDARD
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
    T> : IOverlayCalculation<T>
        where T : ITimedValue
    {
        Task SetAsync(string key, T result, CancellationToken token = default);

        Task<T?> GetAsync(string key, CancellationToken token = default);
    }
}
