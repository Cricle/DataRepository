using DataRepository.Bus;
using System.Diagnostics.CodeAnalysis;

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
    }
}
