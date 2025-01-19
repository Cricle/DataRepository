using DataRepository;
using DataRepository.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

#pragma warning disable IDE0130
namespace Microsoft.Extensions.DependencyInjection
#pragma warning restore IDE0130
{
    public static class EFCoreDataRespositoryServiceCollectionExtensions
    {
        public static IServiceCollection AddRespository<TDbContext>(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
            where TDbContext : DbContext
        {
            services.TryAddScoped<IDbContextStore, DbContextStore<TDbContext>>();
            services.TryAddSingleton<IDataRespositoryCreator, EFDataRespositoryCreator<TDbContext>>();
            services.TryAddScoped(p => p.GetRequiredService<EFDataRespositoryCreator<TDbContext>>().CreateScope());
            services.TryAdd(ServiceDescriptor.Describe(typeof(IDataRespository<>), typeof(DbContextEFRespository<>), serviceLifetime));
            return services;
        }

        internal interface IDbContextStore
        {
            DbContext DbContext { get; }
        }

        internal sealed class DbContextStore<TDbContext>(TDbContext dbContext) : IDbContextStore
            where TDbContext : DbContext
        {
            public DbContext DbContext { get; } = dbContext;
        }

        internal sealed class DbContextEFRespository<TEntity> : EFRespository<TEntity> where TEntity : class
        {
            public DbContextEFRespository(IDbContextStore creator) : base(creator.DbContext)
            {
            }

            public DbContextEFRespository(IDbContextStore creator, IQueryable<TEntity>? query) : base(creator.DbContext, query)
            {
            }
        }
    }
}
