using DataRepository.Casing.Models;

namespace DataRepository.Casing
{
    public interface ITopN<T> : IOverlayCalculation<T>
        where T : ITimedValue
    {
        Task<IList<T>> GetRangeAsync(string key, long? skip = null, long? take = null, bool asc = false, CancellationToken token = default);
    }
}
