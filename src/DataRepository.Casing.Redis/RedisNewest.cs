using DataRepository.Casing.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace DataRepository.Casing.Redis
{
    public class RedisHashNewest<T> : INewest<T>
    {
        private const string TimeKey = "t";
        private const string ValueKey = "v";
        private const string Script = """
            local time = tonumber(redis.call('hget', ARGV[1], 't'))
            local nowTime = tonumber(ARGV[2])
            if time == nil or time < nowTime then
                redis.call('hset', ARGV[1], 't', ARGV[2], 'v', ARGV[3])
                return 1
            end
            return 0
            """;

        private readonly IConnectionMultiplexer connectionMultiplexer;
        private readonly ILogger<RedisHashNewest<T>> logger;
        private LoadedLuaScript? scriptToken;
        private readonly IValuePublisher<T> valuePublisher;
#if NET9_0_OR_GREATER
        private readonly Lock locker = new ();
#else
        private readonly object locker = new ();
#endif

        public RedisHashNewest(IConnectionMultiplexer connectionMultiplexer,
            INewestValueConverter<T> converter,
            ILogger<RedisHashNewest<T>> logger,
            IValuePublisher<T> valuePublisher)
        {
            this.connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
            Converter = converter ?? throw new ArgumentNullException(nameof(converter));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.valuePublisher = valuePublisher ?? throw new ArgumentNullException(nameof(valuePublisher));
        }

        public INewestValueConverter<T> Converter { get; }

        public async Task AddAsync(string key, NewestResult<T> result, CancellationToken token = default)
        {
            await PublishNewestAsync(key, result, token);

            await valuePublisher.PublishAsync(key, result.Value, token);

            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug("Key - {key}, was published {result}", key, result);
        }

        public async Task AddRangesAsync(string key, IEnumerable<NewestResult<T>> results, CancellationToken token = default)
        {
            await PublishNewestAsync(key, results.OrderByDescending(x => x.Time).FirstOrDefault(), token);

            await valuePublisher.PublishAsync(key, results.Select(x => x.Value), token);

            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug("Key - {key}, was publisheds {result}", key, results);
        }

        private async Task PublishNewestAsync(string key, NewestResult<T>? result, CancellationToken token = default)
        {
            await InitAsync(key, token);

            if (result != null)
            {
                var res = await connectionMultiplexer.GetDatabase().ScriptEvaluateAsync(scriptToken!.Hash, null, [key, result.Value.UnixTimeMilliseconds, Converter.Convert(result.Value.Value)]);

                if (logger.IsEnabled(LogLevel.Debug))
                    logger.LogDebug("Script execute finished, result is {@result}", res);
            }
        }

        public async Task<bool> ExistsAsync(string key, CancellationToken token = default)
            => await connectionMultiplexer.GetDatabase().KeyExistsAsync(key);

        public async Task<NewestResult<T>?> GetAsync(string key, CancellationToken token = default)
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
            return new NewestResult<T>(time!.Value, value!);
        }

        public Task InitAsync(string key, CancellationToken token = default)
        {
            if (scriptToken != null) return Task.CompletedTask;
            lock (locker)
            {
                scriptToken ??= LuaScript.Prepare(Script).Load(connectionMultiplexer.GetServers()[0]);
            }
            logger.LogInformation("The script was inited with hash - {@hash}", scriptToken.Hash);
            return Task.CompletedTask;
        }

        public async Task SetAsync(string key, NewestResult<T> result, CancellationToken token)
        {
            await connectionMultiplexer.GetDatabase().HashSetAsync(key,
            [
                new HashEntry(TimeKey,result.UnixTimeMilliseconds),
                new HashEntry(ValueKey,Converter.Convert(result.Value))
            ]);

            logger.LogInformation("Key - {key}, was setted {result}", key, result);
        }
    }
}
