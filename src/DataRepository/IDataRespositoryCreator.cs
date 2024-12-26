namespace DataRepository
{
    public interface IDataRespositoryCreator
    {
        IDataRespository<TEntity> Create<TEntity>()
            where TEntity : class;

        IDataRespositoryScope CreateScope();
    }
}
