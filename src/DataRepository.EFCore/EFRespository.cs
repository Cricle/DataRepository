using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using System.Collections;
using System.Data;
using System.Linq.Expressions;

namespace DataRepository.EFCore
{
    public class EFRespository<TEntity> : IDataRespository<TEntity>
        where TEntity : class
    {
        internal readonly IQueryable<TEntity> query;

        public EFRespository(DbContext context)
            : this(context, null)
        {
        }

        public EFRespository(DbContext context, IQueryable<TEntity>? query)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            this.query = query ?? context.Set<TEntity>().AsNoTracking();
        }

        public DbContext Context { get; }

        public Type ElementType => query.ElementType;

        public Expression Expression => query.Expression;

        public IQueryProvider Provider => query.Provider;

        public IQueryable CreateQuery(Expression expression) => Context.GetService<IAsyncQueryProvider>().CreateQuery(expression);

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => Context.GetService<IAsyncQueryProvider>().CreateQuery<TElement>(expression);

        public object? Execute(Expression expression) => Context.GetService<IAsyncQueryProvider>().Execute(expression);

        public TResult Execute<TResult>(Expression expression) => Context.GetService<IAsyncQueryProvider>().Execute<TResult>(expression);

        public async Task<IDataTransaction> BeginTransactionAsync(IsolationLevel level = IsolationLevel.ReadCommitted, CancellationToken token = default)
        {
            var trans = await Context.Database.BeginTransactionAsync(level, token);
            return new EFDataTransaction(trans);
        }

        #region Operator

        public int Delete(TEntity entity)
        {
            Context.Remove(entity);
            return SaveChanges();
        }

        public async Task<int> DeleteAsync(TEntity entity, CancellationToken token = default)
        {
            Context.Remove(entity);
            return await SaveChangesAsync(token);
        }

        public int Insert(TEntity entity)
        {
            Context.Add(entity);
            return SaveChanges();
        }

        public async Task<int> InsertAsync(TEntity entity, CancellationToken token = default)
        {
            Context.Add(entity);
            return await SaveChangesAsync(token);
        }

        public int Update(TEntity entity)
        {
            Context.Update(entity);
            return SaveChanges();
        }

        public async Task<int> UpdateAsync(TEntity entity, CancellationToken token = default)
        {
            Update(entity);
            return await SaveChangesAsync(token);
        }

        protected virtual Task<int> SaveChangesAsync(CancellationToken token = default) => Context.SaveChangesAsync(token);

        protected virtual int SaveChanges() => Context.SaveChanges();

        #endregion

        #region Batch

        public int InsertMany(IEnumerable<TEntity> entities)
        {
            Context.AddRange(entities);
            return SaveChanges();
        }

        public async Task<int> InsertManyAsync(IEnumerable<TEntity> entities, CancellationToken token = default)
        {
            Context.AddRange(entities);
            return await SaveChangesAsync(token);
        }

        public int DeleteMany(IEnumerable<TEntity> entities)
        {
            Context.RemoveRange(entities);
            return SaveChanges();
        }

        public async Task<int> DeleteManyAsync(IEnumerable<TEntity> entities, CancellationToken token = default)
        {
            Context.RemoveRange(entities);
            return await SaveChangesAsync(token);
        }

        public int UpdateMany(IEnumerable<TEntity> entities)
        {
            Context.UpdateRange(entities);
            return SaveChanges();
        }

        public async Task<int> UpdateManyAsync(IEnumerable<TEntity> entities, CancellationToken token = default)
        {
            Context.UpdateRange(entities);
            return await SaveChangesAsync(token);
        }

        public int UpdateInQuery(Expression expression)
            => query.ExecuteUpdate((Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>>)expression);

        public Task<int> UpdateInQueryAsync(Expression expression, CancellationToken token = default)
            => query.ExecuteUpdateAsync((Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>>)expression, token);

        public int DeleteInQuery()
          => query.ExecuteDelete();

        public Task<int> DeleteInQueryAsync(CancellationToken token = default)
          => query.ExecuteDeleteAsync(token);

        public IDataRespository<TEntity> Where(Expression<Func<TEntity, bool>> expression) => new EFRespository<TEntity>(Context, query.Where(expression));

        public IEnumerator<TEntity> GetEnumerator() => query.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IDataRespository<TNewEntity> Select<TNewEntity>(Expression<Func<TEntity, TNewEntity>> expression)
            where TNewEntity : class
            => new EFRespository<TNewEntity>(Context, query.Select(expression));

        public IUpdateSetBuilder<TEntity> CreateUpdateBuilder() => new EFUpdateSetBuilder<TEntity>();

        public IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken cancellationToken = default) => query.AsAsyncEnumerable().GetAsyncEnumerator(cancellationToken);

        #endregion
    }
}
