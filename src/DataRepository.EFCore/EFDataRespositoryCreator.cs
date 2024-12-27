using Microsoft.EntityFrameworkCore;

namespace DataRepository.EFCore
{
    public sealed class EFDataRespositoryCreator<TContext> : IDataRespositoryCreator
        where TContext : DbContext
    {
        public EFDataRespositoryCreator(IDbContextFactory<TContext> dbContextFactory)
            => DbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));

        public IDbContextFactory<TContext> DbContextFactory { get; }

        public IDataRespository<TEntity> Create<TEntity>()
            where TEntity : class => new EFRespository<TEntity>(DbContextFactory.CreateDbContext());

        public IDataRespositoryScope CreateScope() => new EFDataRespositoryScope<TContext>(DbContextFactory.CreateDbContext());
    }
}
