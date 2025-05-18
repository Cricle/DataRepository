using DataRepository.Casing;
#if !NETSTANDARD
using System.Diagnostics.CodeAnalysis;
#endif

namespace DataRepository.Builder
{
    public interface ICastingBuilder<
#if !NETSTANDARD
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
    TValue>
    {
        IServiceProvider Provider { get; }

        int CalculationCount { get; }

        ICastingBuilder<TValue> Add(IOverlayCalculation<TValue> overlayCalculation);

        ICastingBuilder<TValue> Add<T>()
            where T: IOverlayCalculation<TValue>;

        void Clear();
    }
}
