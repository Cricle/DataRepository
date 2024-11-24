using DataRepository;
using DataRepository.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

internal class Program
{
    private static void Main(string[] args)
    {
        var factory = LoggerFactory.Create(x => x.AddConsole());
        var options = new DbContextOptionsBuilder<StudentDbContext>()
            .UseSqlite("DataSource=file::memory:?cache=shared")
            .UseLoggerFactory(factory)
            .Options;
        var dbc = new StudentDbContext(options);
        dbc.Database.EnsureCreated();
        dbc.Students.Add(new Student { Id = 1, Name = "a" });
        dbc.SaveChanges();
        dbc.ChangeTracker.Clear();
        var rep = new EFRespository<Student>(dbc);
        var a = rep.Where(x => x.Id == 1)
            .UpdateInQuery(x => x.Set(y => y.Name, y => dbc.Students.Where(q => q.Id == 1).Count() + "2"));
        rep.Where(x => x.Id == 1).Count();
        Console.WriteLine(a);
        Console.WriteLine(rep.FirstOrDefaultAsync().Result);
    }

    public record class Student
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class StudentDbContext : DbContext
    {
        public StudentDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Student> Students => Set<Student>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Student>(b =>
            {
                b.ToTable("student");
            });
        }
    }
}