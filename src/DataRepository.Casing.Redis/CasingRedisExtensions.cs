using DataRepository.Casing;
using DataRepository.Casing.Redis;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text.Json;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CasingRedisExtensions
    {
        public static IServiceCollection AddRedisCasting(this IServiceCollection services, Type valuePublisherType, JsonSerializerOptions? jsonSerializerOptions = null)
        {
            services.TryAdd(ServiceDescriptor.Describe(typeof(IValuePublisher<>), valuePublisherType, ServiceLifetime.Singleton));
            services.TryAdd(ServiceDescriptor.Describe(typeof(ICasingNewest<>), typeof(RedisHashCasingNewest<>), ServiceLifetime.Singleton));
            services.TryAdd(ServiceDescriptor.Describe(typeof(INewestValueConverter<>), typeof(DIJsonNewestValueConverter<>), ServiceLifetime.Singleton));
            services.TryAddSingleton(new JsonOptionsBox(jsonSerializerOptions ?? JsonSerializerOptions.Default));
            return services;
        }

        public static IServiceCollection AddRedisNewest(this IServiceCollection services,RedisHashCasingNewestConfig newestConfig)
        {
            services.TryAdd(ServiceDescriptor.Describe(typeof(ICasingNewest<>), typeof(RedisHashCasingNewest<>), ServiceLifetime.Singleton));
            services.TryAddSingleton(newestConfig);
            return services;
        }

        public static IServiceCollection AddRedisTopN(this IServiceCollection services, RedisSortSetTopNConfig topNConfig)
        {
            services.TryAdd(ServiceDescriptor.Describe(typeof(ITopN<>), typeof(RedisSortSetTopN<>), ServiceLifetime.Singleton));
            services.TryAddSingleton(topNConfig);
            return services;
        }

        internal sealed class JsonOptionsBox
        {
            public JsonOptionsBox(JsonSerializerOptions jsonSerializerOptions)
            {
                JsonSerializerOptions = jsonSerializerOptions;
            }

            public JsonSerializerOptions JsonSerializerOptions { get; }
        }

        internal sealed class DIJsonNewestValueConverter<TModel> : JsonNewestValueConverter<TModel>
        {
            public DIJsonNewestValueConverter(JsonOptionsBox box) 
                : base(box.JsonSerializerOptions)
            {
            }
        }
    }
}
