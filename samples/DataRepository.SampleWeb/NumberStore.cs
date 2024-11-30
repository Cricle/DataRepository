using Microsoft.EntityFrameworkCore;

namespace DataRepository.SampleWeb
{
    public class NumberDbContext : DbContext
    {
        public NumberDbContext(DbContextOptions options) : base(options)
        {
        }

        protected NumberDbContext()
        {
        }

        public DbSet<Number> Numbers => Set<Number>();
    }
}
