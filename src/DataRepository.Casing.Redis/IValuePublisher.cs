namespace DataRepository.Casing.Redis
{
    public interface IValuePublisher<T>
    {
        Task<int> PublishAsync(string key, T value, CancellationToken token = default);

        Task<int> PublishAsync(string key, IEnumerable<T> values, CancellationToken token = default);
    }
}
