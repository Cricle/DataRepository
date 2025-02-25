using DataRepository.Casing.Models;

namespace DataRepository.Casing
{
    public interface ITopN<T> : IOverlayCalculation<TimedResult<T>, T>
    {
        Task<long> CountAsync(string key, CancellationToken token = default);

        Task<IList<T>> GetRangeAsync(string key, long? skip, long? take, bool asc, CancellationToken token = default);
    }
}
