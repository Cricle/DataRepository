using DataRepository.Casing.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
#if !NETSTANDARD
using System.Diagnostics.CodeAnalysis;
#endif

namespace DataRepository.Casing.Redis
{
    public class RedisSortSetTopN<
#if !NETSTANDARD
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
    T> : RedisOverlayCalculation<T>, ITopN<T>
        where T : ITimedValue
    {
        private readonly INewestValueConverter<T> converter;
        private readonly RedisSortSetTopNConfig topNConfig;

        public RedisSortSetTopN(RedisSortSetTopNConfig topNConfig,
            IConnectionMultiplexer connectionMultiplexer,
            ILogger<RedisSortSetTopN<T>> logger,
            INewestValueConverter<T> converter)
            : base(connectionMultiplexer, logger)
        {
            this.topNConfig = topNConfig;
            this.converter = converter;
        }

        public override async Task<long> CountAsync(string key, CancellationToken token = default)
            => await connectionMultiplexer.GetDatabase().SortedSetLengthAsync(GetKey(key));

        public async Task<IList<T>> GetRangeAsync(string key, long? skip = null, long? take = null, bool asc = false, CancellationToken token = default)
        {
            var order = asc ? Order.Ascending : Order.Descending;
            var end = (skip ?? 0) + (take ?? 0);
            if (end == 0) end = -1;
            var values = await connectionMultiplexer.GetDatabase().SortedSetRangeByRankAsync(GetKey(key), skip ?? 0L, end, order);
            var results = new T[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                results[i] = converter.ConvertBack(values[i])!;
            }
            return results;
        }

        protected override async Task OverlayNewValuesAsync(string key, IEnumerable<T> results, CancellationToken token = default)
        {
            var count = results.Count();
            var keys = new RedisKey[count][];
            var constKey = new RedisKey[] { GetKey(key) };
            keys.AsSpan().Fill(constKey);
            var values = new RedisValue[count][];

            var index = 0;
            foreach (var item in results)
                values[index++] = [GetUnixTimeMilliseconds(item.GetTime()), converter.Convert(item)];

            await ScriptEvaluateAsync(count, keys, values, CommandFlags.None);
        }

        protected override async Task OverlayNewValuesAsync(string key, T result, CancellationToken token = default)
            => await ScriptEvaluateAsync([GetKey(key)], [GetUnixTimeMilliseconds(result.GetTime()), converter.Convert(result)]);

        protected internal override string GetScript()
        {
            return $$"""
                local key = KEYS[1]
                local score = ARGV[1]
                local member = ARGV[2]

                redis.call('ZADD', key, score, member)
                if redis.call('ZCARD', key) > {{topNConfig.StoreSize}} then
                    redis.call('ZREMRANGEBYRANK', key, 0, count - {{topNConfig.StoreSize - 1}})
                end
                """;
        }

        private static long GetUnixTimeMilliseconds(DateTime time)
        {
            return new DateTimeOffset(time.ToUniversalTime()).ToUnixTimeMilliseconds();
        }

        protected override string GetKey(string key) => $"{topNConfig.Prefx}{key}";
    }
}
