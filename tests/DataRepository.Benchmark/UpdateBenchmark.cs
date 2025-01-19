using BenchmarkDotNet.Attributes;
using DataRepository.EFCore;
using Microsoft.EntityFrameworkCore;

namespace DataRepository.Benchmark
{
    [MemoryDiagnoser]
    public class UpdateBenchmark
    {
        private TheDbContext dbContext;
        private EFRespository<Student> students;

        [GlobalSetup]
        public void Setup()
        {
            dbContext = new TheDbContext(new DbContextOptionsBuilder<TheDbContext>().UseSqlite("DataSource=file::memory:").Options);
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            students = new EFRespository<Student>(dbContext);
            dbContext.Students.Add(new Student { Name = "a", A = 0 });
            dbContext.SaveChanges();
            dbContext.ChangeTracker.Clear();
        }

        [Benchmark(Baseline = true)]
        public async Task UpdateRaw()
        {
            await dbContext.Set<Student>().Where(x => x.Name == "a").ExecuteUpdateAsync(x => x.SetProperty(y => y.A, y => y.A + 1));
        }

        [Benchmark]
        public async Task UpdateProxy()
        {
            await students.Where(x => x.Name == "a").ExecuteUpdateAsync(x => x.SetProperty(y => y.A, y => y.A + 1));
        }
    }
}
