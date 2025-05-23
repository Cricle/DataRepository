﻿using StackExchange.Redis;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace DataRepository.Casing.Redis
{
    public class JsonNewestValueConverter<
#if !NETSTANDARD
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif

    T> : INewestValueConverter<T>
    {
        private readonly JsonSerializerOptions options;

        public JsonNewestValueConverter() : this(JsonSerializerOptions.Default) { }

        public JsonNewestValueConverter(JsonSerializerOptions options) => this.options = options ?? throw new ArgumentNullException(nameof(options));

        public RedisValue Convert(T? value) => JsonSerializer.Serialize(value, options);

        public T? ConvertBack(RedisValue value) => value.HasValue ? JsonSerializer.Deserialize<T>(value!, options) : default;
    }
}
