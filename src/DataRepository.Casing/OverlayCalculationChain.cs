#if !NETSTANDARD
using System.Diagnostics.CodeAnalysis;
#endif

namespace DataRepository.Casing
{
    public class OverlayCalculationChain<
#if !NETSTANDARD
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
    TValue> : List<IOverlayCalculation<TValue>>, IOverlayCalculation<TValue>
    {
        private readonly IValuePublisher<TValue> valuePublisher;

        public OverlayCalculationChain(IValuePublisher<TValue> valuePublisher)
        {
            this.valuePublisher = valuePublisher;
        }

        public bool IsInited => this.All(x => x.IsInited);

        public async Task AddAsync(string key, TValue result, CancellationToken token = default)
        {
            foreach (var item in this)
            {
                await item.AddAsync(key, result, token);
            }
            await valuePublisher.PublishAsync(key, result, token);
        }

        public async Task AddRangesAsync(string key, IEnumerable<TValue> results, CancellationToken token = default)
        {
            foreach (var item in this)
            {
                await item.AddRangesAsync(key, results, token);
            }
            await valuePublisher.PublishAsync(key, results, token);
        }

        public async Task<long> CountAsync(string key, CancellationToken token = default)
        {
            var sum = 0L;
            foreach (var item in this)
            {
                sum += await item.CountAsync(key, token);
            }
            return sum;
        }

        public async Task DeInitAsync()
        {
            foreach (var item in this)
            {
                await item.DeInitAsync();
            }
        }

        public async Task DeleteAsync(string key, CancellationToken token = default)
        {
            foreach (var item in this)
            {
                await item.DeleteAsync(key, token);
            }
        }

        public async Task<bool> ExistsAsync(string key, CancellationToken token = default)
        {
            foreach (var item in this)
            {
                if (await item.ExistsAsync(key,token))
                {
                    return true;
                }
            }
            return false;
        }

        public async Task InitAsync()
        {
            foreach (var item in this)
            {
                await item.InitAsync();
            }
        }
    }
}
