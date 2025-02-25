using DataRepository.Casing.Models;

namespace DataRepository.Casing
{
    public interface ICasingNewest<T> : IOverlayCalculation<TimedResult<T>, T>
    {
        Task SetAsync(string key, TimedResult<T> result, CancellationToken token = default);

        Task<TimedResult<T>?> GetAsync(string key, CancellationToken token = default);
    }
}
