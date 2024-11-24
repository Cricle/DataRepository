﻿using DataRepository;
using DataRepository.EFCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

internal class Program
{
    private static void Main(string[] args)
    {
        var options=new DbContextOptionsBuilder<StudentDbContext>()
            .UseSqlite("DataSource=file::memory:?cache=shared")
            .Options;
        var dbc = new StudentDbContext(options);
        dbc.Database.EnsureCreated();
        dbc.Students.Add(new Student { Id = 1, Name = "a" });
        dbc.SaveChanges();
        dbc.ChangeTracker.Clear();
        var rep=new EFRespository<Student>(dbc);
        var a = rep.UpdateInQuery(x => x.Set(y => y.Name, y => dbc.Students.Where(q => q.Id == 1).Count() + "2"));
        Console.WriteLine(a);
        Console.WriteLine(rep.FirstOrDefaultAsync().Result);
    }

    public record class Student
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class StudentDbContext:DbContext
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