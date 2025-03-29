using DataRepository.SampleWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace DataRepository.SampleWeb
{
    public class NumberDbContext : DbContext
    {
        public NumberDbContext(DbContextOptions options) : base(options)
        {
        }

        public NumberDbContext()
        {
        }

        public DbSet<Number> Numbers => Set<Number>();

        public DbSet<GpsPosition> GpsPositions => Set<GpsPosition>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<GpsPosition>(x =>
            {
                x.HasKey(x => new { x.DeviceId, x.Time });
            });
        }
    }
}
