using DataRepository.Casing;
using Microsoft.Extensions.DependencyInjection;

namespace DataRepository.Builder
{
    public interface ICastingBuilder<TValue>
    {
        IServiceProvider Provider { get; }

        int CalculationCount { get; }

        ICastingBuilder<TValue> Add(IOverlayCalculation<TValue> overlayCalculation);

        ICastingBuilder<TValue> Add<T>()
            where T: IOverlayCalculation<TValue>;

        void Clear();
    }
}
