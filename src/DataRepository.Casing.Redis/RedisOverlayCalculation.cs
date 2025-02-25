using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace DataRepository.Casing.Redis
{
    public abstract class RedisOverlayCalculation<TWraper, TValue> : RedisLuaScripter, IOverlayCalculation<TWraper, TValue>
    {
        private readonly IValuePublisher<TValue> valuePublisher;

        protected RedisOverlayCalculation(IConnectionMultiplexer connectionMultiplexer,
            ILogger logger,
            IValuePublisher<TValue> valuePublisher)
            : base(connectionMultiplexer, logger)
        {
            this.valuePublisher = valuePublisher;
        }

        public async Task AddAsync(string key, TWraper result, CancellationToken token = default)
        {
            await OverlayNewValuesAsync(key, result, token).ConfigureAwait(false);
            await valuePublisher.PublishAsync(key, ToValue(result), token).ConfigureAwait(false);
            await OnAddAsync(key, result, token).ConfigureAwait(false);
        }

        protected virtual Task OnAddAsync(string key, TWraper result, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        public async Task AddRangesAsync(string key, IEnumerable<TWraper> results, CancellationToken token = default)
        {
            if (!results.Any()) return;

            await OverlayNewValuesAsync(key, results, token).ConfigureAwait(false);
            await valuePublisher.PublishAsync(key, results.Select(ToValue), token).ConfigureAwait(false);
            await OnAddRangesAsync(key, results, token).ConfigureAwait(false);
        }

        protected virtual Task OnAddRangesAsync(string key, IEnumerable<TWraper> results, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        public virtual async Task DeleteAsync(string key, CancellationToken token = default)
            => await connectionMultiplexer.GetDatabase().KeyDeleteAsync(key);

        public virtual async Task<bool> ExistsAsync(string key, CancellationToken token = default)
            => await connectionMultiplexer.GetDatabase().KeyExistsAsync(key);

        protected virtual async Task OverlayNewValuesAsync(string key, TWraper result, CancellationToken token = default)
            => await OverlayNewValuesAsync(key, Enumerable.Repeat(result, 1), token);

        protected abstract Task OverlayNewValuesAsync(string key, IEnumerable<TWraper> results, CancellationToken token = default);

        protected abstract TValue ToValue(TWraper value);

    }
}
