#if !NETSTANDARD
using System.Diagnostics.CodeAnalysis;
#endif

namespace DataRepository
{
    public interface IDataRespositoryCreator
    {
        IDataRespository<TEntity> Create<
#if !NETSTANDARD
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif

        TEntity>()
            where TEntity : class;

        IDataRespositoryScope CreateScope();
    }
}
