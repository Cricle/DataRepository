using DataRepository.Bus;
using DataRepository.Bus.Nats;
using DataRepository.Bus.Serialization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using NATS.Net;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class NatsServiceExtensions
    {
        public static IServiceCollection AddNatsJsonMessage(this IServiceCollection services, JsonSerializerOptions? options)
        {
            services.TryAddSingleton<IMessageSerialization>(new JsonMessageSerialization(options ?? JsonSerializerOptions.Default));
            return services;
        }

        public static IServiceCollection AddMessageConsumer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]TMessage, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TImpl>(this IServiceCollection services)
            where TImpl:class,IConsumer<TMessage>
            where TMessage:class
        {
            services.AddScoped<IConsumer<TMessage>, TImpl>();
            return services;
        }

        public static IServiceCollection AddRequestReply<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TRequest,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TReply,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TImpl>(this IServiceCollection services)
            where TImpl : class, IRequestReply<TRequest, TReply>
        {
            services.AddScoped<IRequestReply<TRequest,TReply>, TImpl>();
            return services;
        }

        public static IServiceCollection AddNatsBus(this IServiceCollection services, Action<NatsDispatcherBuilder> consumerConfig, Action<IServiceProvider, NatsOpts>? optsConfig = null)
        {
            services.TryAddSingleton<INatsConnection>(p =>
            {
                var jsOpt = new NatsOpts
                {
                    LoggerFactory = p.GetRequiredService<ILoggerFactory>(),
                    ObjectPoolSize = 128,
                };
                optsConfig?.Invoke(p, jsOpt);
                return new NatsConnection(jsOpt);
            });

            services.TryAddSingleton<IBus>(p =>
            {
                var natsConnection = p.GetRequiredService<INatsConnection>();
                var consumerBuilder = new NatsDispatcherBuilder(p.GetRequiredService<ILoggerFactory>(), p.GetRequiredService<IMessageSerialization>(), natsConnection);
                consumerConfig(consumerBuilder);
                return new NatsBus(natsConnection, p.GetRequiredService<ILogger<NatsBus>>(),
                    p.GetRequiredService<IMessageSerialization>(), p.GetRequiredService<IServiceScopeFactory>(), consumerBuilder);
            });
            return services;
        }
    }
}
