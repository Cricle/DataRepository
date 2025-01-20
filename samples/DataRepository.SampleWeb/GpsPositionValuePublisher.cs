using DataRepository.Casing;
using DataRepository.SampleWeb.Models;

namespace DataRepository.SampleWeb
{
    internal class GpsPositionValuePublisher(IDataRespositoryCreator dataRespositoryCreator) : IValuePublisher<GpsPosition>
    {
        public async Task<int> PublishAsync(string key, GpsPosition value, CancellationToken token = default)
            => await dataRespositoryCreator.Create<GpsPosition>().InsertAsync(value, token);

        public async Task<int> PublishAsync(string key, IEnumerable<GpsPosition> values, CancellationToken token = default)
            => await dataRespositoryCreator.Create<GpsPosition>().InsertManyAsync(values, token);
    }
}
