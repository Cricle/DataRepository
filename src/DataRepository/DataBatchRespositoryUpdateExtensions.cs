﻿using DataRepository.Models;

namespace DataRepository
{
    public static class DataBatchRespositoryUpdateExtensions
    {
        public static int ExecuteUpdate<TEntity>(this IDataBatchRespository<TEntity> respository, Action<IUpdateSetBuilder<TEntity>> action)
        {
            var builder = respository.CreateUpdateBuilder();
            action(builder);
            return respository.ExecuteUpdate(builder.Build());
        }

        public static Task<int> ExecuteUpdateAsync<TEntity>(this IDataBatchRespository<TEntity> respository, Action<IUpdateSetBuilder<TEntity>> action, CancellationToken token = default)
        {
            var builder = respository.CreateUpdateBuilder();
            action(builder);
            return respository.ExecuteUpdateAsync(builder.Build(), token);
        }

        public static IDataRespository<TEntity> Page<TEntity>(this IDataRespository<TEntity> respository, int pageIndex, int pageSize)
            where TEntity : class
            => respository.Skip((pageIndex - 1) * pageSize).Take(pageSize);

        public static IDataRespository<TEntity> Page<TEntity>(this IDataRespository<TEntity> respository, IPageQueryInfo pageQueryInfo)
            where TEntity : class
            => Page(respository, pageQueryInfo.PageIndex, pageQueryInfo.PageSize);

        public static async Task<IWorkPageResult<TEntity>> PageQueryAsync<TEntity>(this IDataRespository<TEntity> respository, IPageQueryInfo pageQueryInfo, CancellationToken token = default)
            where TEntity : class
            => await PageQueryAsync(respository, pageQueryInfo.PageIndex, pageQueryInfo.PageSize, token);

        public static async Task<IWorkPageResult<TEntity>> PageQueryAsync<TEntity>(this IDataRespository<TEntity> respository, int pageIndex, int pageSize, CancellationToken token = default)
            where TEntity : class
        {
            if (pageIndex < 1 || pageSize <= 0) return new WorkPageResult<TEntity>([], 0, pageIndex, 0);

            var count = await respository.CountAsync(token);
            if (count <= 0) return new WorkPageResult<TEntity>([], 0, pageIndex, 0);

            var datas = await Page(respository,pageIndex,pageSize).ToListAsync(token);

            return new WorkPageResult<TEntity>(datas, count, pageIndex, pageSize);
        }
    }
}
