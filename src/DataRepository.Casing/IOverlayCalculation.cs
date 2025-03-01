namespace DataRepository.Casing
{
    public interface IOverlayCalculation<TValue> : IInitable
    {
        Task AddAsync(string key, TValue result, CancellationToken token = default);

        Task AddRangesAsync(string key, IEnumerable<TValue> results, CancellationToken token = default);

        Task DeleteAsync(string key, CancellationToken token = default);

        Task<bool> ExistsAsync(string key, CancellationToken token = default);

        Task<long> CountAsync(string key, CancellationToken token = default);
    }
}
