#if !NETSTANDARD
using System.Diagnostics.CodeAnalysis;
#endif

namespace DataRepository.Casing
{
    public sealed class EmptyValuePublisher<
#if !NETSTANDARD
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
    T> : IValuePublisher<T>
    {
        public static EmptyValuePublisher<T> Instance { get; } = new EmptyValuePublisher<T>();

        private EmptyValuePublisher() { }

        private static readonly Task<int> zeroResult = Task.FromResult(0);

        public Task<int> PublishAsync(string key, T value, CancellationToken token = default) => zeroResult;

        public Task<int> PublishAsync(string key, IEnumerable<T> values, CancellationToken token = default) => zeroResult;
    }
}
