using DataRepository.Casing;
using Microsoft.Extensions.DependencyInjection;
#if !NETSTANDARD
using System.Diagnostics.CodeAnalysis;
#endif

namespace DataRepository.Builder
{
    public sealed class DefaultCastingBuilder<
#if !NETSTANDARD
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
        TValue> : ICastingBuilder<TValue>
    {
        public DefaultCastingBuilder(IServiceProvider provider,IValuePublisher<TValue> valuePublisher)
        {
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            Chain = new OverlayCalculationChain<TValue>(valuePublisher);
        }

        public OverlayCalculationChain<TValue> Chain { get; }

        public IServiceProvider Provider { get; }

        public int CalculationCount => Chain.Count;

        public ICastingBuilder<TValue> Add(IOverlayCalculation<TValue> overlayCalculation)
        {
            Chain.Add(overlayCalculation);
            return this;
        }

        public ICastingBuilder<TValue> Add<T>() where T : IOverlayCalculation<TValue>
            => Add(Provider.GetRequiredService<T>());

        public IOverlayCalculation<TValue> Build()
        {
            return Chain;
        }

        public void Clear()
        {
            Chain.Clear();
        }
    }
}
