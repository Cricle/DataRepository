using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Specialized;

namespace DataRepository.Casing.Redis
{
    public abstract class RedisLuaScripter : IInitable
    {
        internal LoadedLuaScript? scriptToken;

        protected readonly ILogger logger;
        protected readonly IConnectionMultiplexer connectionMultiplexer;
#if NET9_0_OR_GREATER
        protected readonly Lock locker = new();
#else
        protected readonly object locker = new();
#endif
        public bool IsInited => scriptToken != null;

        protected RedisLuaScripter(IConnectionMultiplexer connectionMultiplexer, ILogger logger)
        {
            this.connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public LoadedLuaScript? GetLoadedLuaScript() => scriptToken;

        public Task InitAsync()
        {
            if (scriptToken != null) return Task.CompletedTask;
            lock (locker)
            {
                scriptToken ??= LuaScript.Prepare(GetScript()).Load(connectionMultiplexer.GetServers()[0]);
            }
            logger.LogInformation("The script was inited with hash - {@hash}", scriptToken.Hash);
            return Task.CompletedTask;
        }

        protected internal abstract string GetScript();

        protected async Task ScriptEvaluateAsync(RedisKey[]? keys = null, RedisValue[]? values = null, CommandFlags flags = CommandFlags.None)
        {
            await InitAsync();

            var res = await connectionMultiplexer.GetDatabase().ScriptEvaluateAsync(scriptToken!.Hash, keys, values, flags);

            if (logger.IsEnabled(LogLevel.Debug)) logger.LogDebug("Script execute finished, result is {@result}", res);
        }

        protected async Task ScriptEvaluateAsync(int count, RedisKey[][] keys, RedisValue[][] values, CommandFlags flags = CommandFlags.None)
        {
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count), "The count must more than zero");
            if (keys == null || values == null) throw new ArgumentException($"The count is {count}, but keys or values null");
            if (keys.Length != count || values.Length != count) throw new ArgumentException($"The count is {count}, but keys count {keys.Length} and values count {values.Length}");

            await InitAsync();

            var db = connectionMultiplexer.GetDatabase();
            var batch = db.CreateBatch();
            var tasks = new Task[keys.Length];
            for (int i = 0; i < count; i++)
            {
                tasks[i] = batch.ScriptEvaluateAsync(scriptToken!.Hash, keys[i], values[i], flags);
            }

            batch.Execute();
            await Task.WhenAll(tasks);

            if (logger.IsEnabled(LogLevel.Debug)) logger.LogDebug("Script batch execute finished");
        }

        public Task DeInitAsync()
        {
            scriptToken = null;
            return Task.CompletedTask;
        }
    }
}
