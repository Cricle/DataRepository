using System.Linq.Expressions;
#if !NETSTANDARD
using System.Diagnostics.CodeAnalysis;
#endif

namespace DataRepository
{
    public interface IUpdateSetBuilder<
#if !NETSTANDARD
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
    TEntity>
    {
        int SetCount { get; }

        Expression Build();

        IUpdateSetBuilder<TEntity> SetProperty<TProperty>(Expression<Func<TEntity, TProperty>> selector, TProperty value);

        IUpdateSetBuilder<TEntity> SetProperty<TProperty>(Expression<Func<TEntity, TProperty>> selector, Expression<Func<TEntity, TProperty>> valueExp);
    }
}
