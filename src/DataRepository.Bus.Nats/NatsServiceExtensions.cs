using DataRepository.Bus;
using DataRepository.Bus.Nats;
using DataRepository.Bus.Serialization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class NatsServiceExtensions
    {

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
                return new NatsBus(natsConnection, p.GetRequiredService<IServiceScopeFactory>(), p.GetRequiredService<IMessageSerialization>(), p.GetRequiredService<ILogger<NatsBus>>(),consumerBuilder);
            });
            return services;
        }
    }
}
