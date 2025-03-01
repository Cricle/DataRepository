using DataRepository.Casing.Models;

namespace DataRepository.Casing
{
    public interface ICasingNewest<T> : IOverlayCalculation<T>
        where T : ITimedValue
    {
        Task SetAsync(string key, T result, CancellationToken token = default);

        Task<T?> GetAsync(string key, CancellationToken token = default);
    }
}
