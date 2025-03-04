using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace DataRepository.Casing.Redis
{
    public abstract class RedisOverlayCalculation<TValue> : RedisLuaScripter, IOverlayCalculation<TValue>
    {
        protected RedisOverlayCalculation(IConnectionMultiplexer connectionMultiplexer,
            ILogger logger)
            : base(connectionMultiplexer, logger)
        {
        }

        public async Task AddAsync(string key, TValue result, CancellationToken token = default)
        {
            await OverlayNewValuesAsync(key, result, token).ConfigureAwait(false);
            await OnAddAsync(key, result, token).ConfigureAwait(false);
        }

        protected virtual Task OnAddAsync(string key, TValue result, CancellationToken token = default) => Task.CompletedTask;

        public async Task AddRangesAsync(string key, IEnumerable<TValue> results, CancellationToken token = default)
        {
            if (!results.Any()) return;

            await OverlayNewValuesAsync(key, results, token).ConfigureAwait(false);
            await OnAddRangesAsync(key, results, token).ConfigureAwait(false);
        }

        protected virtual Task OnAddRangesAsync(string key, IEnumerable<TValue> results, CancellationToken token = default) => Task.CompletedTask;

        public virtual async Task DeleteAsync(string key, CancellationToken token = default)
            => await connectionMultiplexer.GetDatabase().KeyDeleteAsync(GetKey(key));

        public virtual async Task<bool> ExistsAsync(string key, CancellationToken token = default)
            => await connectionMultiplexer.GetDatabase().KeyExistsAsync(GetKey(key));

        protected virtual async Task OverlayNewValuesAsync(string key, TValue result, CancellationToken token = default)
            => await OverlayNewValuesAsync(key, Enumerable.Repeat(result, 1), token);

        protected abstract Task OverlayNewValuesAsync(string key, IEnumerable<TValue> results, CancellationToken token = default);

        public abstract Task<long> CountAsync(string key, CancellationToken token = default);

        protected virtual string GetKey(string key) => key;
    }
}
