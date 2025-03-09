using DataRepository.Masstransit.Models;
using MassTransit;

namespace DataRepository.Masstransit
{
    public class StackingDataService<TEntity> : StackingDataService<TEntity, TEntity>,IStackingDataService<TEntity>
        where TEntity : class
    {
        public StackingDataService(IBus bus, IDataRespository<TEntity> dataRespository) : base(bus, dataRespository)
        {
        }

        protected override TEntity ToEntity(TEntity transfer) => transfer;
    }

    public abstract class StackingDataService<TTransfer, TEntity> : IStackingDataService<TTransfer, TEntity> where TEntity : class
    {
        public IBus Bus { get; }
        public IDataRespository<TEntity> DataRespository { get; }

        protected StackingDataService(IBus bus, IDataRespository<TEntity> dataRespository)
        {
            Bus = bus;
            DataRespository = dataRespository;
        }

        public async Task AddAsync(TTransfer transfer, StackingDataPublishType publishType= StackingDataPublishType.Entity, CancellationToken token = default)
        {
            await AddRangeAsync([transfer], publishType, token);
        }

        public async Task<int> AddRangeAsync(List<TTransfer> transfers, StackingDataPublishType publishType = StackingDataPublishType.Entity, CancellationToken token = default)
        {
            if (transfers == null) throw new ArgumentNullException(nameof(transfers));
            if (transfers.Count==0) throw new ArgumentException($"The transfers is empty");

            var entities = transfers.ConvertAll(ToEntity);
            await OnAddingAsync(transfers, entities);
            var insertedCount=await DataRespository.InsertManyAsync(entities, token);

            if (publishType == StackingDataPublishType.Transfer)
                await Bus.Publish(new AddedResult<TTransfer> { Count = insertedCount, Datas = transfers }, token);
            else
                await Bus.Publish(new AddedResult<TEntity> { Count = insertedCount, Datas = entities }, token);

            await OnAddedAsync(transfers, entities);
            return insertedCount;
        }

        protected virtual Task OnAddingAsync(List<TTransfer> transfers, List<TEntity> entities)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnAddedAsync(List<TTransfer> transfers, List<TEntity> entities)
        {
            return Task.CompletedTask;
        }

        public async Task<int> DeleteAsync(IDataRespository<TEntity> dataQuery, List<string>? conditions = null, bool publishMsg = true, CancellationToken token = default)
        {
            if (dataQuery == null) throw new ArgumentNullException(nameof(dataQuery));

            await OnDeletingingAsync(dataQuery);
            var deletedCount = await dataQuery.ExecuteDeleteAsync(token);

            if (publishMsg)
                await Bus.Publish(new DeletedResult<TEntity> { Count = deletedCount, Conditions = conditions }, token);

            await OnDeletedAsync(dataQuery, deletedCount);
            return deletedCount;
        }

        protected virtual Task OnDeletingingAsync(IDataRespository<TEntity> dataQuery)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnDeletedAsync(IDataRespository<TEntity> dataQuery, int deletedCount)
        {
            return Task.CompletedTask;
        }

        public async Task<int> UpdateAsync(IDataRespository<TEntity> dataQuery, Action<IUpdateSetBuilder<TEntity>> updateProperties, List<string>? conditions = null, bool publishMsg = true, CancellationToken token = default)
        {
            if (dataQuery == null) throw new ArgumentNullException(nameof(dataQuery));
            if (updateProperties == null) throw new ArgumentNullException(nameof(updateProperties));

            await OnUpdatingAsync(dataQuery);
            var updatedCount = await dataQuery.ExecuteUpdateAsync(updateProperties, token);

            if (publishMsg)
                await Bus.Publish(new UpdatedResult<TEntity> { Count = updatedCount, Conditions = conditions }, token);

            await OnUpdatedAsync(dataQuery, updatedCount);
            return updatedCount;
        }

        protected virtual Task OnUpdatingAsync(IDataRespository<TEntity> dataQuery)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnUpdatedAsync(IDataRespository<TEntity> dataQuery, int deletedCount)
        {
            return Task.CompletedTask;
        }

        protected abstract TEntity ToEntity(TTransfer transfer);
    }
}
