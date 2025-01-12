using DataRepository.Casing.Models;

namespace DataRepository.Casing
{
    public interface INewest<T>
    {
        Task InitAsync(string key, CancellationToken token=default);

        Task SetAsync(string key, NewestResult<T> result, CancellationToken token);

        Task<NewestResult<T>?> GetAsync(string key, CancellationToken token = default);

        Task<bool> ExistsAsync(string key, CancellationToken token = default);

        Task AddAsync(string key, NewestResult<T> result, CancellationToken token = default);

        Task AddRangesAsync(string key, IEnumerable<NewestResult<T>> results, CancellationToken token = default);
    }
}
