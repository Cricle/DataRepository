using Microsoft.EntityFrameworkCore;
using Moq.EntityFrameworkCore;

namespace Moq
{
    internal static class EFSetupExtensions
    {
        public static void SetupDbSet<T, TEntity>(this Mock<T> setup, List<TEntity> entities)
            where T : DbContext
            where TEntity : class
        {
            setup.Setup(x => x.Set<TEntity>()).ReturnsDbSet(entities);
        }
    }
}
