﻿using DataRepository.Builder;
using DataRepository.Casing;
using DataRepository.Casing.Models;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CastingBuilderExtensions
    {
        public static IServiceCollection AddCasting<TValue>(this IServiceCollection services, Action<ICastingBuilder<TValue>> action, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
        {
            services.Add(ServiceDescriptor.Describe(typeof(IOverlayCalculation<TValue>), provider =>
            {
                var publish = provider.GetRequiredService<IValuePublisher<TValue>>();
                var builder = new DefaultCastingBuilder<TValue>(provider, publish);
                action(builder);
                return builder.Build();

            }, serviceLifetime));
            return services;
        }

        public static ICastingBuilder<TValue> AddTopN<TValue>(this ICastingBuilder<TValue> builder)
            where TValue : ITimedValue
            => builder.Add<ITopN<TValue>>();

        public static ICastingBuilder<TValue> AddNewest<TValue>(this ICastingBuilder<TValue> builder)
            where TValue : ITimedValue
            => builder.Add<ICasingNewest<TValue>>();

        public static T? FindCalculation<T, TValue>(this IOverlayCalculation<TValue> calculation)
            where T : IOverlayCalculation<TValue>
        {
            if (calculation is T result)
            {
                return result;
            }
            if (calculation is IEnumerable<IOverlayCalculation<TValue>> results)
            {
                foreach (var item in results)
                {
                    var value = FindCalculation<T, TValue>(item);
                    if (!Equals(value, default))
                    {
                        return value;
                    }
                }
            }
            return default;
        }

        public static ITopN<TValue>? FindTopN<TValue>(this IOverlayCalculation<TValue> calculation)
            where TValue : ITimedValue
            => FindCalculation<ITopN<TValue>, TValue>(calculation);

        public static ICasingNewest<TValue>? FindNewest<TValue>(this IOverlayCalculation<TValue> calculation)
            where TValue : ITimedValue
            => FindCalculation<ICasingNewest<TValue>, TValue>(calculation);
    }
}
