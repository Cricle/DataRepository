using Microsoft.EntityFrameworkCore;

namespace DataRespository.MasstransitSample
{
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Student> Students => Set<Student>();
    }
}
