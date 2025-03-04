using DataRepository.Casing.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace DataRepository.Casing.Redis
{
    public class RedisHashCasingNewest<T> : RedisOverlayCalculation<T>, ICasingNewest<T>
        where T : ITimedValue
    {
        internal const string TimeKey = "t";
        internal const string ValueKey = "v";

        private readonly RedisHashCasingNewestConfig newestConfig;

        public RedisHashCasingNewest(RedisHashCasingNewestConfig newestConfig,
            IConnectionMultiplexer connectionMultiplexer,
            INewestValueConverter<T> converter,
            ILogger<RedisHashCasingNewest<T>> logger)
            : base(connectionMultiplexer, logger)
        {
            this.newestConfig = newestConfig ?? throw new ArgumentNullException(nameof(newestConfig));
            Converter = converter ?? throw new ArgumentNullException(nameof(converter));
        }

        public INewestValueConverter<T> Converter { get; }

        private static long GetUnixTimeMilliseconds(DateTime time)
        {
            return new DateTimeOffset(time.ToUniversalTime()).ToUnixTimeMilliseconds();
        }

        protected override async Task OverlayNewValuesAsync(string key, T result, CancellationToken token = default)
            => await ScriptEvaluateAsync(null, [GetKey(key), GetUnixTimeMilliseconds(result.GetTime()), Converter.Convert(result)], CommandFlags.None);

        protected override async Task OverlayNewValuesAsync(string key, IEnumerable<T> results, CancellationToken token = default)
        {
            if (!results.Any()) return;
            await OverlayNewValuesAsync(key, results.OrderByDescending(x => GetUnixTimeMilliseconds(x.GetTime())).First(), token);
        }

        public async Task<T?> GetAsync(string key, CancellationToken token = default)
        {
            var sets = await connectionMultiplexer.GetDatabase().HashGetAllAsync(GetKey(key));
            if (sets.Length == 0) return default;

            foreach (var set in sets)
            {
                if (set.Name == ValueKey) return Converter.ConvertBack(set.Value);
            }

            return default;
        }

        public async Task SetAsync(string key, T result, CancellationToken token = default)
        {
            await connectionMultiplexer.GetDatabase().HashSetAsync(GetKey(key),
            [
                new HashEntry(TimeKey,GetUnixTimeMilliseconds(result.GetTime())),
                new HashEntry(ValueKey,Converter.Convert(result))
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

        public override async Task<long> CountAsync(string key, CancellationToken token = default)
            => await ExistsAsync(key, token) ? 1L : 0L;

        protected override string GetKey(string key) => $"{newestConfig.Prefx}{key}";
    }
}
