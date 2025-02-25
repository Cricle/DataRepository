using DataRepository.Casing;
using DataRepository.Casing.Redis;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text.Json;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CasingRedisExtensions
    {
        public static void AddRedisNewest<TModel, TPublisher>(this IServiceCollection services, JsonSerializerOptions? jsonSerializerOptions = null)
            where TPublisher : class, IValuePublisher<TModel>
        {
            services.TryAddSingleton<IValuePublisher<TModel>, TPublisher>();
            services.TryAddSingleton<ICasingNewest<TModel>, RedisHashCasingNewest<TModel>>();
            services.TryAddSingleton<INewestValueConverter<TModel>>(new JsonNewestValueConverter<TModel>(jsonSerializerOptions ?? JsonSerializerOptions.Default));
        }

        public static void AddRedisTopN<TModel, TPublisher>(this IServiceCollection services, JsonSerializerOptions? jsonSerializerOptions = null)
           where TPublisher : class, IValuePublisher<TModel>
        {
            services.TryAddSingleton<IValuePublisher<TModel>, TPublisher>();
            services.TryAddSingleton<ICasingNewest<TModel>, RedisHashCasingNewest<TModel>>();
            services.TryAddSingleton<INewestValueConverter<TModel>>(new JsonNewestValueConverter<TModel>(jsonSerializerOptions ?? JsonSerializerOptions.Default));
        }
    }
}
