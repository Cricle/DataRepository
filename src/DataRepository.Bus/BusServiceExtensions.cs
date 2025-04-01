using DataRepository.Bus;
using DataRepository.Bus.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class BusServiceExtensions
    {
        public static IServiceCollection AddMessageConsumer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TMessage, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TImpl>(this IServiceCollection services)
            where TImpl : class, IBatchConsumer<TMessage>
            where TMessage : class
        {
            services.AddScoped<IBatchConsumer<TMessage>, TImpl>();
            return services;
        }

        public static IServiceCollection AddRequestReply<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TRequest,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TReply,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TImpl>(this IServiceCollection services)
            where TImpl : class, IRequestReply<TRequest, TReply>
        {
            services.AddScoped<IRequestReply<TRequest, TReply>, TImpl>();
            return services;
        }

        public static IServiceCollection AddJsonMessageSerializer(this IServiceCollection services, JsonSerializerOptions? options = null)
        {
            services.AddSingleton<IMessageSerialization>(new JsonMessageSerialization(options));
            return services;
        }
    }
}
