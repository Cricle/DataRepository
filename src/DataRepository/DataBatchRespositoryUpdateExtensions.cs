namespace DataRepository
{
    public static class DataBatchRespositoryUpdateExtensions
    {
        public static int UpdateInQuery<TEntity>(this IDataBatchRespository<TEntity> respository, Action<IUpdateSetBuilder<TEntity>> action)
        {
            var builder = respository.CreateUpdateBuilder();
            action(builder);
            var exp = builder.Build();
            return respository.UpdateInQuery(exp);
        }

        public static Task<int> UpdateInQueryAsync<TEntity>(this IDataBatchRespository<TEntity> respository, Action<IUpdateSetBuilder<TEntity>> action, CancellationToken token = default)
        {
            var builder = respository.CreateUpdateBuilder();
            action(builder);
            return respository.UpdateInQueryAsync(builder.Build(), token);
        }
    }
}
