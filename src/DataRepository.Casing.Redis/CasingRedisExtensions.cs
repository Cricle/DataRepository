using DataRepository.Casing;
using DataRepository.Casing.Redis;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text.Json;
using DataRepository.Casing.Models;
#if !NETSTANDARD
using System.Diagnostics.CodeAnalysis;
#endif

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CasingRedisExtensions
    {
        public static IServiceCollection AddRedisCasting<
#if !NETSTANDARD
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
        TEntity,
#if !NETSTANDARD
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
        TPublish>(this IServiceCollection services, JsonSerializerOptions? jsonSerializerOptions = null)
            where TPublish : class, IValuePublisher<TEntity>
            where TEntity : ITimedValue
        {
            services.TryAddSingleton<IValuePublisher<TEntity>, TPublish>();
            services.TryAddSingleton<ICasingNewest<TEntity>, RedisHashCasingNewest<TEntity>>();
            services.TryAddSingleton<INewestValueConverter<TEntity>, DIJsonNewestValueConverter<TEntity>>();
            services.TryAddSingleton(new JsonOptionsBox(jsonSerializerOptions ?? JsonSerializerOptions.Default));
            return services;
        }
        public static IServiceCollection AddRedisCasting(this IServiceCollection services, Type valuePublisherType, JsonSerializerOptions? jsonSerializerOptions = null)
        {
            services.TryAdd(ServiceDescriptor.Describe(typeof(IValuePublisher<>), valuePublisherType, ServiceLifetime.Singleton));
            services.TryAdd(ServiceDescriptor.Describe(typeof(ICasingNewest<>), typeof(RedisHashCasingNewest<>), ServiceLifetime.Singleton));
            services.TryAdd(ServiceDescriptor.Describe(typeof(INewestValueConverter<>), typeof(DIJsonNewestValueConverter<>), ServiceLifetime.Singleton));
            services.TryAddSingleton(new JsonOptionsBox(jsonSerializerOptions ?? JsonSerializerOptions.Default));
            return services;
        }

        public static IServiceCollection AddRedisNewest<
#if !NETSTANDARD
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
        TEntity>(this IServiceCollection services, RedisHashCasingNewestConfig newestConfig)
            where TEntity : ITimedValue
        {
            services.TryAddSingleton<ICasingNewest<TEntity>,RedisHashCasingNewest<TEntity>>();
            services.TryAddSingleton(newestConfig);
            return services;
        }

        public static IServiceCollection AddRedisNewest(this IServiceCollection services, RedisHashCasingNewestConfig newestConfig)
        {
            services.TryAdd(ServiceDescriptor.Describe(typeof(ICasingNewest<>), typeof(RedisHashCasingNewest<>), ServiceLifetime.Singleton));
            services.TryAddSingleton(newestConfig);
            return services;
        }

        public static IServiceCollection AddRedisTopN<
#if !NETSTANDARD
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
        TEntity>(this IServiceCollection services, RedisSortSetTopNConfig topNConfig)
            where TEntity : ITimedValue
        {
            services.TryAddSingleton<ITopN<TEntity>,RedisSortSetTopN<TEntity>>();
            services.TryAddSingleton(topNConfig);
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

        internal sealed class DIJsonNewestValueConverter<
#if !NETSTANDARD
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
        TModel> : JsonNewestValueConverter<TModel>
        {
            public DIJsonNewestValueConverter(JsonOptionsBox box)
                : base(box.JsonSerializerOptions)
            {
            }
        }
    }
}
