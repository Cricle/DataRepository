using DataRepository.Casing;

namespace DataRepository.SampleWeb
{
    internal class ValuePublisher<T>(IDataRespositoryCreator dataRespositoryCreator) : IValuePublisher<T>
        where T : class
    {
        public async Task<int> PublishAsync(string key, T value, CancellationToken token = default)
            => await dataRespositoryCreator.Create<T>().InsertAsync(value, token);

        public async Task<int> PublishAsync(string key, IEnumerable<T> values, CancellationToken token = default)
            => await dataRespositoryCreator.Create<T>().InsertManyAsync(values, token);
    }
}
