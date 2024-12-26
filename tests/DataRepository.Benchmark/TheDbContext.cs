using Microsoft.EntityFrameworkCore;

namespace DataRepository.Benchmark
{
    public class TheDbContext : DbContext
    {
        public TheDbContext(DbContextOptions options) : base(options)
        {
        }

        protected TheDbContext()
        {
        }

        public virtual DbSet<Student> Students { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Student>(b =>
            {
                b.HasKey(x => x.Name);
            });
        }
    }
}
