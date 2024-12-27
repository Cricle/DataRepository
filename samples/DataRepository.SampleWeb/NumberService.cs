using DataRepository.Models;

namespace DataRepository.SampleWeb
{
    public class NumberService(IDataRespository<Number> respository)
    {
        public async Task<IWorkPageResult<Number>> PageAsync(int pageIndex, int pageSize) => await respository.PageQueryAsync(pageIndex, pageSize);

        public async Task<WorkDataResult<bool>> InsertAsync(int id, int value)
            => await respository.InsertAsync(new Number { Id = id, Value = value }) > 0;

        public async Task<WorkDataResult<bool>> UpdateAsync(int id, int value)
            => await respository.Where(x => x.Id == id).ExecuteUpdateAsync(x => x.SetProperty(y => y.Value, value)) > 0;

        public async Task<WorkDataResult<bool>> IncreaseAsync(int id, int value)
            => await respository.Where(x => x.Id == id).ExecuteUpdateAsync(x => x.SetProperty(y => y.Value, y => y.Value + value)) > 0;

        public async Task<WorkDataResult<bool>> DeleteAsync(int id)
            => await respository.Where(x => x.Id == id).ExecuteDeleteAsync() > 0;
    }
}
