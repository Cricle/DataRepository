using DataRepository.Casing;
using MassTransit;

namespace DataRepository.Masstransit
{
    public sealed class MasstransitValuePublisher<T>(IBus Bus) : IValuePublisher<T>
        where T : class
    {
        public async Task<int> PublishAsync(string key, T value, CancellationToken token = default)
        {
            await Bus.Publish(value, token);
            return 1;
        }

        public async Task<int> PublishAsync(string key, IEnumerable<T> values, CancellationToken token = default)
        {
            await Bus.PublishBatch(values, token);
            return values.Count();
        }
    }
}
