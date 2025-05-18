using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace DataRepository.EFCore
{
    public sealed class EFDataRespositoryCreator<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TContext> : IDataRespositoryCreator
        where TContext : DbContext
    {
        public EFDataRespositoryCreator(IDbContextFactory<TContext> dbContextFactory)
            => DbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));

        public IDbContextFactory<TContext> DbContextFactory { get; }

        public IDataRespository<TEntity> Create<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TEntity>()
            where TEntity : class => new EFRespository<TEntity>(DbContextFactory.CreateDbContext());

        public IDataRespositoryScope CreateScope() => new EFDataRespositoryScope<TContext>(DbContextFactory.CreateDbContext());
    }
}
