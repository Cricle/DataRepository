using MassTransit;

namespace DataRepository.Masstransit
{
    public class StackingDataService<TEntity> : StackingDataService<TEntity, TEntity>
        where TEntity : class
    {
        public StackingDataService(IBus bus, IDataRespository<TEntity> dataRespository, StackingDataServiceConfig stackingDataServiceConfig) : base(bus, dataRespository, stackingDataServiceConfig)
        {
        }

        protected override TEntity ToEntity(TEntity transfer) => transfer;
    }

    public abstract class StackingDataService<TTransfer, TEntity>
        where TEntity : class
    {
        protected readonly IBus bus;
        protected readonly IDataRespository<TEntity> dataRespository;
        protected readonly StackingDataServiceConfig stackingDataServiceConfig;

        protected StackingDataService(IBus bus, IDataRespository<TEntity> dataRespository, StackingDataServiceConfig stackingDataServiceConfig)
        {
            this.bus = bus;
            this.dataRespository = dataRespository;
            this.stackingDataServiceConfig = stackingDataServiceConfig;
        }

        public async Task AddAsync(TTransfer transfer, CancellationToken token = default)
        {
            if (transfer == null) throw new ArgumentNullException(nameof(transfer));

            var entity = ToEntity(transfer);
            await OnAddingAsync(transfer, entity);
            await dataRespository.InsertAsync(entity, token);

            if (stackingDataServiceConfig.PublishType == StackingDataPublishType.Transfer)
                await bus.Publish(transfer, token);
            else
                await bus.Publish(entity, token);

            await OnAddedAsync(transfer, entity);
        }

        protected virtual Task OnAddingAsync(TTransfer transfer, TEntity entity)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnAddedAsync(TTransfer transfer, TEntity entity)
        {
            return Task.CompletedTask;
        }

        protected abstract TEntity ToEntity(TTransfer transfer);
    }
}
