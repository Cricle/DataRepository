using DataRepository.Masstransit;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DataMasstransitServiceExtensions
    {
        public static IServiceCollection AddStackDataService(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            services.TryAdd(ServiceDescriptor.Describe(typeof(IStackingDataService<>), typeof(StackingDataService<>), serviceLifetime));
            return services;
        }
    }
}
