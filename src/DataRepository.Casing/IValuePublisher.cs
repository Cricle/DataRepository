#if !NETSTANDARD
using System.Diagnostics.CodeAnalysis;
#endif

namespace DataRepository.Casing
{
    public interface IValuePublisher<
#if !NETSTANDARD
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
    T>
    {
        Task<int> PublishAsync(string key, T value, CancellationToken token = default);

        Task<int> PublishAsync(string key, IEnumerable<T> values, CancellationToken token = default);
    }
}
