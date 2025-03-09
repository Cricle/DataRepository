using MassTransit;

namespace DataRepository.Masstransit
{
    public interface IStackingDataService<TEntity>:IStackingDataService<TEntity,TEntity> where TEntity : class
    {

    }
    public interface IStackingDataService<TTransfer, TEntity> where TEntity : class
    {
        IBus Bus { get; }

        IDataRespository<TEntity> DataRespository { get; }

        Task AddAsync(TTransfer transfer, StackingDataPublishType publishType = StackingDataPublishType.Entity, CancellationToken token = default);

        Task<int> AddRangeAsync(List<TTransfer> transfers, StackingDataPublishType publishType = StackingDataPublishType.Entity, CancellationToken token = default);

        Task<int> DeleteAsync(IDataRespository<TEntity> dataQuery, List<string>? conditions = null, bool publishMsg = true, CancellationToken token = default);
      
        Task<int> UpdateAsync(IDataRespository<TEntity> dataQuery, Action<IUpdateSetBuilder<TEntity>> updateProperties, List<string>? conditions = null, bool publishMsg = true, CancellationToken token = default);
    }
}