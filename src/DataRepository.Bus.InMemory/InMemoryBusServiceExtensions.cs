using DataRepository.Bus;
using DataRepository.Bus.InMemory;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class InMemoryBusServiceExtensions
    {
        public static IServiceCollection AddInMemoryBus(this IServiceCollection services, Action<InMemoryDispatcherBuilder> consumerConfig)
        {
            services.TryAddSingleton<IBus>(p =>
            {
                var consumerBuilder = new InMemoryDispatcherBuilder(p.GetRequiredService<ILoggerFactory>());
                consumerConfig(consumerBuilder);
                return new InMemoryBus(p.GetRequiredService<IServiceScopeFactory>(), p.GetRequiredService<ILogger<InMemoryBus>>(), consumerBuilder);
            });
            return services;
        }
    }
}
