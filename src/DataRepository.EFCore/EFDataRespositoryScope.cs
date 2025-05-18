using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace DataRepository.EFCore
{
    public sealed class EFDataRespositoryScope<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TContext> : IDataRespositoryScope
        where TContext : DbContext
    {
        public EFDataRespositoryScope(TContext context) => Context = context ?? throw new ArgumentNullException(nameof(context));

        public TContext Context { get; }

        public bool SupportDbConnection => true;

        public IDataRespository<TEntity> Create<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TEntity>() where TEntity : class => new EFRespository<TEntity>(Context);

        public void Dispose() => Context.Dispose();

        public IDbConnection GetConnection() => Context.Database.GetDbConnection();
    }
}
