namespace DataRepository
{
    public interface IDataRespositoryScope : IDisposable
    {
        IDataRespository<TEntity> Create<TEntity>()
            where TEntity : class;
    }
}
