using DataRepository.Casing.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace DataRepository.Casing.Redis
{
    public class RedisSortSetTopN<T> : RedisOverlayCalculation<TimedResult<T>, T>, ITopN<T>
    {
        private readonly INewestValueConverter<T> converter;
        private readonly int storeSize;

        public RedisSortSetTopN(int storeSize,
            IConnectionMultiplexer connectionMultiplexer,
            ILogger logger,
            IValuePublisher<T> valuePublisher,
            INewestValueConverter<T> converter)
            : base(connectionMultiplexer, logger, valuePublisher)
        {
            if (storeSize <= 0) throw new ArgumentOutOfRangeException(nameof(storeSize), "The storeSize must more than 0");
            this.storeSize = storeSize;
            this.converter = converter;
        }

        public async Task<long> CountAsync(string key, CancellationToken token = default)
            => await connectionMultiplexer.GetDatabase().SortedSetLengthAsync(key);

        public async Task<IList<T>> GetRangeAsync(string key, long? skip, long? take, bool asc, CancellationToken token = default)
        {
            var order = asc ? Order.Ascending : Order.Descending;
            var end = (skip ?? 0) + (take ?? 0);
            if (end == 0) end = -1;
            var values = await connectionMultiplexer.GetDatabase().SortedSetRangeByRankAsync(key, skip ?? 0L, end, order);
            var results = new T[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                results[i] = converter.ConvertBack(values[i])!;
            }
            return results;
        }

        protected override async Task OverlayNewValuesAsync(string key, IEnumerable<TimedResult<T>> results, CancellationToken token = default)
        {
            var count = results.Count();
            var keys = new RedisKey[count][];
            var constKey = new RedisKey[] { key };
            keys.AsSpan().Fill(constKey);
            var values = new RedisValue[count][];

            var index = 0;
            foreach (var item in results)
                values[index++] = [item.UnixTimeMilliseconds, converter.Convert(item.Value)];

            await ScriptEvaluateAsync(count, keys, values, CommandFlags.None);
        }

        protected override async Task OverlayNewValuesAsync(string key, TimedResult<T> result, CancellationToken token = default)
            => await ScriptEvaluateAsync([key], [result.UnixTimeMilliseconds, converter.Convert(result.Value)]);

        protected internal override string GetScript()
        {
            return $$"""
                local key = KEYS[1]
                local score = ARGV[1]
                local member = ARGV[2]

                redis.call('ZADD', key, score, member)
                if redis.call('ZCARD', key) > {{storeSize}} then
                    redis.call('ZREMRANGEBYRANK', key, 0, count - {{storeSize - 1}})
                end
                """;
        }

        protected override T ToValue(TimedResult<T> value) => value.Value;
    }
}
