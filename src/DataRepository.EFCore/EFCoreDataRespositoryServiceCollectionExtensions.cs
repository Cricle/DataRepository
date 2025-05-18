﻿using DataRepository;
using DataRepository.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130
namespace Microsoft.Extensions.DependencyInjection
#pragma warning restore IDE0130
{
    public static class EFCoreDataRespositoryServiceCollectionExtensions
    {
        public static IServiceCollection AddRespository<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TDbContext>(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
            where TDbContext : DbContext
        {
            services.TryAddScoped<IDbContextStore, DbContextStore<TDbContext>>();
            services.TryAddSingleton<IDataRespositoryCreator, EFDataRespositoryCreator<TDbContext>>();
            services.TryAddScoped(p => p.GetRequiredService<EFDataRespositoryCreator<TDbContext>>().CreateScope());
            services.TryAdd(ServiceDescriptor.Describe(typeof(IDataRespository<>), typeof(DbContextEFRespository<>), serviceLifetime));
            return services;
        }

        public static IServiceCollection AddKeyRespository<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TDbContext>(this IServiceCollection services, object? key, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
            where TDbContext : DbContext
        {
            services.TryAddKeyedScoped<IDbContextStore, DbContextStore<TDbContext>>(key);
            services.TryAddKeyedSingleton<IDataRespositoryCreator, EFDataRespositoryCreator<TDbContext>>(key);
            services.TryAddKeyedScoped(key, (p, _) => p.GetRequiredService<EFDataRespositoryCreator<TDbContext>>().CreateScope());
            services.TryAdd(ServiceDescriptor.DescribeKeyed(typeof(IDataRespository<>), key, typeof(DbContextEFRespository<>), serviceLifetime));
            return services;
        }

        internal interface IDbContextStore
        {
            DbContext DbContext { get; }
        }

        internal sealed class DbContextStore<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TDbContext>(TDbContext dbContext) : IDbContextStore
            where TDbContext : DbContext
        {
            public DbContext DbContext { get; } = dbContext;
        }

        internal sealed class DbContextEFRespository<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TEntity> : EFRespository<TEntity> where TEntity : class
        {
            public DbContextEFRespository(IDbContextStore creator) : base(creator.DbContext)
            {
            }
        }
    }
}
