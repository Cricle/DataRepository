using DataRepository.Models;

namespace DataRepository.SampleWeb
{
    public class NumberService
    {
        private readonly IDataRespository<Number> _respository;

        public NumberService(IDataRespository<Number> respository)
        {
            _respository = respository;
        }

        public async Task<IWorkPageResult<Number>> PageAsync(int pageIndex, int pageSize) => await _respository.PageQueryAsync(pageIndex, pageSize);

        public async Task<WorkDataResult<bool>> InsertAsync(int id, int value)
        {
            var result = await _respository.Where(x => x.Id == id).InsertAsync(new Number { Id = id, Value = value });
            return result > 0;
        }
        public async Task<WorkDataResult<bool>> UpdateAsync(int id, int value)
        {
            var result = await _respository.Where(x => x.Id == id).UpdateInQueryAsync(x => x.Set(y => y.Value, value));
            return result > 0;
        }

        public async Task<WorkDataResult<bool>> DeleteAsync(int id)
        {
            var result = await _respository.Where(x => x.Id == id).DeleteInQueryAsync();
            return result > 0;
        }
    }
}
