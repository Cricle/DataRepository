using DataRepository.Casing.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace DataRepository.Casing.Redis
{
    public class RedisHashCasingNewest<T> : RedisOverlayCalculation<TimedResult<T>, T>, ICasingNewest<T>
    {
        internal const string TimeKey = "t";
        internal const string ValueKey = "v";

        public RedisHashCasingNewest(IConnectionMultiplexer connectionMultiplexer,
            INewestValueConverter<T> converter,
            ILogger<RedisHashCasingNewest<T>> logger,
            IValuePublisher<T> valuePublisher)
            : base(connectionMultiplexer, logger, valuePublisher)
        {
            Converter = converter ?? throw new ArgumentNullException(nameof(converter));
        }

        public INewestValueConverter<T> Converter { get; }

        protected override Task OnAddAsync(string key, TimedResult<T> result, CancellationToken token = default)
        {
            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug("Key - {key}, was published {result}", key, result);
            return Task.CompletedTask;
        }

        protected override async Task OverlayNewValuesAsync(string key, TimedResult<T> result, CancellationToken token = default)
            => await ScriptEvaluateAsync(null, [key, result.UnixTimeMilliseconds, Converter.Convert(result.Value)], CommandFlags.None);

        protected override async Task OverlayNewValuesAsync(string key, IEnumerable<TimedResult<T>> results, CancellationToken token = default)
            => await OverlayNewValuesAsync(key, results.OrderByDescending(x => x.Time).First(), token);

        public async Task<TimedResult<T>?> GetAsync(string key, CancellationToken token = default)
        {
            var sets = await connectionMultiplexer.GetDatabase().HashGetAllAsync(key);
            if (sets.Length == 0)
                return null;

            var hitCount = 0;
            DateTime? time = null;
            T? value = default;
            foreach (var set in sets)
            {
                if (set.Name == TimeKey)
                {
                    time = DateTimeOffset.FromUnixTimeMilliseconds((long)set.Value).DateTime;
                    hitCount++;
                }
                else if (set.Name == ValueKey)
                {
                    value = Converter.ConvertBack(set.Value);
                    hitCount++;
                }
            }

            if (hitCount < 2) return null;
            return new TimedResult<T>(time!.Value, value!);
        }

        public async Task SetAsync(string key, TimedResult<T> result, CancellationToken token = default)
        {
            await connectionMultiplexer.GetDatabase().HashSetAsync(key,
            [
                new HashEntry(TimeKey,result.UnixTimeMilliseconds),
                new HashEntry(ValueKey,Converter.Convert(result.Value))
            ]);

            logger.LogInformation("Key - {key}, was setted {result}", key, result);
        }

        protected internal override string GetScript()
        {
            return """
            local time = tonumber(redis.call('hget', ARGV[1], 't'))
            local nowTime = tonumber(ARGV[2])
            if time == nil or time < nowTime then
                redis.call('hset', ARGV[1], 't', ARGV[2], 'v', ARGV[3])
                return 1
            end
            return 0
            """;
        }

        protected override T ToValue(TimedResult<T> value) => value.Value;
    }
}
