using Microsoft.EntityFrameworkCore;

namespace DataRepository.EFCore
{
    public sealed class EFDataRespositoryScope<TContext> : IDataRespositoryScope
        where TContext : DbContext
    {
        public EFDataRespositoryScope(TContext context) => Context = context ?? throw new ArgumentNullException(nameof(context));

        public TContext Context { get; }

        public IDataRespository<TEntity> Create<TEntity>() where TEntity : class => new EFRespository<TEntity>(Context);

        public void Dispose() => (Context as IDisposable)?.Dispose();
    }
}
