using DataRepository.Casing.Models;
#if !NETSTANDARD
using System.Diagnostics.CodeAnalysis;
#endif

namespace DataRepository.Casing
{
    public interface ITopN<
#if !NETSTANDARD
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
    T> : IOverlayCalculation<T>
        where T : ITimedValue
    {
        Task<IList<T>> GetRangeAsync(string key, long? skip = null, long? take = null, bool asc = false, CancellationToken token = default);
    }
}
