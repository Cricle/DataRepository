using DataRepository.Casing;
using DataRepository.Casing.Redis;
using System.Text.Json;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CasingRedisExtensions
    {
        public static void AddRedisNewest<TModel, TPublisher>(this IServiceCollection services, JsonSerializerOptions? jsonSerializerOptions = null)
            where TPublisher : class, IValuePublisher<TModel>
        {
            services.AddSingleton<IValuePublisher<TModel>, TPublisher>();
            services.AddSingleton<INewest<TModel>, RedisHashNewest<TModel>>();
            services.AddSingleton<INewestValueConverter<TModel>>(new JsonNewestValueConverter<TModel>(jsonSerializerOptions ?? JsonSerializerOptions.Default));
        }
    }
}
