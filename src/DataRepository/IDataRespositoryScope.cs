namespace DataRepository
{
    public interface IDataRespositoryScope : IDbConnectionProvider, IDisposable
    {
        IDataRespository<TEntity> Create<TEntity>()
            where TEntity : class;
    }
}
