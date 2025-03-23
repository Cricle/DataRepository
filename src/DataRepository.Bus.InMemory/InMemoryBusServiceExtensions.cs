using DataRepository.Bus;
using DataRepository.Bus.InMemory;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class InMemoryBusServiceExtensions
    {
        public static IServiceCollection AddMessageConsumer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TMessage, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TImpl>(this IServiceCollection services)
            where TImpl : class, IConsumer<TMessage>
            where TMessage : class
        {
            services.AddScoped<IConsumer<TMessage>, TImpl>();
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

        public static IServiceCollection AddInMemoryBus(this IServiceCollection services, Action<InMemoryDispatcherBuilder> consumerConfig)
        {
            services.TryAddSingleton<IBus>(p =>
            {
                var consumerBuilder = new InMemoryDispatcherBuilder(p.GetRequiredService<ILoggerFactory>());
                consumerConfig(consumerBuilder);
                return new InMemoryBus(consumerBuilder, p.GetRequiredService<ILogger<InMemoryBus>>(),p.GetRequiredService<IServiceScopeFactory>());
            });
            return services;
        }
    }
}
