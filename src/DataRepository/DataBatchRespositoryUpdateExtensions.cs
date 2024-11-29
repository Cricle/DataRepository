using DataRepository.Models;

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

        public static async Task<IWorkPageResult<TEntity>> PageQueryAsync<TEntity>(this IDataRespository<TEntity> respository, int pageIndex, int pageSize, CancellationToken token = default)
            where TEntity : class
        {
            if (pageIndex < 1 || pageSize <= 0) return new WorkPageResult<TEntity>([], 0, pageIndex, 0);

            var count = await respository.CountAsync(token);
            if (count <= 0) return new WorkPageResult<TEntity>([], 0, pageIndex, 0);

            var datas = await respository.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(token);

            return new WorkPageResult<TEntity>(datas, count, pageIndex, pageSize);
        }
    }
}
