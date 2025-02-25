namespace DataRepository.Casing
{
    public interface IOverlayCalculation<TWraper, TValue> : IInitable
    {
        Task AddAsync(string key, TWraper result, CancellationToken token = default);

        Task AddRangesAsync(string key, IEnumerable<TWraper> results, CancellationToken token = default);

        Task DeleteAsync(string key, CancellationToken token = default);

        Task<bool> ExistsAsync(string key, CancellationToken token = default);
    }
}
