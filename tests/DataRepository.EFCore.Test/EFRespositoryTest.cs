﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace DataRepository.EFCore.Test
{
    public class EFRespositoryTest
    {
        private readonly Mock<TheDbContext> theDbContext = new();

        [Theory, AutoData]
        public void Delete_RemoveTheEntity(List<Student> students)
        {
            theDbContext.SetupDbSet(students);

            var sut = GetSut();
            var res = sut.Delete(students[0]);

            res.Should().Be(0);
            theDbContext.Verify(x => x.Remove(students[0]));
            theDbContext.Verify(x => x.SaveChanges());
        }

        [Theory, AutoData]
        public async Task DeleteAsync_RemoveTheEntity(List<Student> students)
        {
            theDbContext.SetupDbSet(students);

            var sut = GetSut();
            var res = await sut.DeleteAsync(students[0]);

            res.Should().Be(0);
            theDbContext.Verify(x => x.Remove(students[0]));
            theDbContext.Verify(x => x.SaveChangesAsync(default));
        }

        [Theory, AutoData]
        public void Insert_InsertTheEntity(Student student)
        {
            theDbContext.SetupDbSet(new List<Student>());

            var sut = GetSut();
            var res = sut.Insert(student);

            res.Should().Be(0);
            theDbContext.Verify(x => x.Add(student));
            theDbContext.Verify(x => x.SaveChanges());
        }

        [Theory, AutoData]
        public async Task InsertAsync_InsertTheEntity(Student student)
        {
            theDbContext.SetupDbSet(new List<Student>());

            var sut = GetSut();
            var res = await sut.InsertAsync(student);

            res.Should().Be(0);
            theDbContext.Verify(x => x.Add(student));
            theDbContext.Verify(x => x.SaveChangesAsync(default));
        }

        [Theory, AutoData]
        public void Update_UpdateTheEntity(Student student)
        {
            theDbContext.SetupDbSet(new List<Student>());

            var sut = GetSut();
            var res = sut.Update(student);

            res.Should().Be(0);
            theDbContext.Verify(x => x.Update(student));
            theDbContext.Verify(x => x.SaveChanges());
        }

        [Theory, AutoData]
        public async Task UpdateAsync_UpdateTheEntity(Student student)
        {
            theDbContext.SetupDbSet(new List<Student>());

            var sut = GetSut();
            var res = await sut.UpdateAsync(student);

            res.Should().Be(0);
            theDbContext.Verify(x => x.Update(student));
            theDbContext.Verify(x => x.SaveChangesAsync(default));
        }

        [Theory, AutoData]
        public void AddMany_AddThereEntity(List<Student> students)
        {
            theDbContext.SetupDbSet(new List<Student>());

            var sut = GetSut();
            var res = sut.InsertMany(students);

            res.Should().Be(0);
            theDbContext.Verify(x => x.AddRange(students));
            theDbContext.Verify(x => x.SaveChanges());
        }

        [Theory, AutoData]
        public async Task AddManyAsync_AddThereEntity(List<Student> students)
        {
            theDbContext.SetupDbSet(new List<Student>());

            var sut = GetSut();
            var res = await sut.InsertManyAsync(students);

            res.Should().Be(0);
            theDbContext.Verify(x => x.AddRange(students));
            theDbContext.Verify(x => x.SaveChangesAsync(default));
        }

        [Theory, AutoData]
        public void DeleteMany_DeleteThereEntity(List<Student> students)
        {
            theDbContext.SetupDbSet(new List<Student>());

            var sut = GetSut();
            var res = sut.DeleteMany(students);

            res.Should().Be(0);
            theDbContext.Verify(x => x.RemoveRange(students));
            theDbContext.Verify(x => x.SaveChanges());
        }

        [Theory, AutoData]
        public async Task DeleteManyAsync_DeleteThereEntity(List<Student> students)
        {
            theDbContext.SetupDbSet(new List<Student>());

            var sut = GetSut();
            var res = await sut.DeleteManyAsync(students);

            res.Should().Be(0);
            theDbContext.Verify(x => x.RemoveRange(students));
            theDbContext.Verify(x => x.SaveChangesAsync(default));
        }

        [Theory, AutoData]
        public void UpdateMany_UpdateThereEntity(List<Student> students)
        {
            theDbContext.SetupDbSet(new List<Student>());

            var sut = GetSut();
            var res = sut.UpdateMany(students);

            res.Should().Be(0);
            theDbContext.Verify(x => x.UpdateRange(students));
            theDbContext.Verify(x => x.SaveChanges());
        }

        [Theory, AutoData]
        public async Task UpdateManyAsync_UpdateThereEntity(List<Student> students)
        {
            theDbContext.SetupDbSet(new List<Student>());

            var sut = GetSut();
            var res = await sut.UpdateManyAsync(students);

            res.Should().Be(0);
            theDbContext.Verify(x => x.UpdateRange(students));
            theDbContext.Verify(x => x.SaveChangesAsync(default));
        }

        [Theory, AutoData]
        public async Task CountAsync_CountThereEntity(List<Student> students)
        {
            theDbContext.SetupDbSet(students);

            var sut = GetSut();
            var res = await sut.CountAsync();

            res.Should().Be(students.Count);
            theDbContext.Verify(x => x.Set<Student>());
        }

        [Theory, AutoData]
        public async Task LongCountAsync_LongCountThereEntity(List<Student> students)
        {
            theDbContext.SetupDbSet(students);

            var sut = GetSut();
            var res = await sut.LongCountAsync();

            res.Should().Be(students.Count);
            theDbContext.Verify(x => x.Set<Student>());
        }

        [Theory, AutoData]
        public async Task FirstOrDefaultAsync_FindFirst(List<Student> students)
        {
            theDbContext.SetupDbSet(students);

            var sut = GetSut();
            var res = await sut.FirstOrDefaultAsync();

            res.Should().Be(students[0]);
            theDbContext.Verify(x => x.Set<Student>());
        }

        [Fact]
        public async Task FirstOrDefaultAsync_Empty_ReturnNull()
        {
            theDbContext.SetupDbSet(new List<Student>());

            var sut = GetSut();
            var res = await sut.FirstOrDefaultAsync();

            res.Should().BeNull();
            theDbContext.Verify(x => x.Set<Student>());
        }

        [Theory, AutoData]
        public async Task LastOrDefaultAsync_FindLast(List<Student> students)
        {
            theDbContext.SetupDbSet(students);

            var sut = GetSut();
            var res = await sut.LastOrDefaultAsync();

            res.Should().Be(students[students.Count-1]);
            theDbContext.Verify(x => x.Set<Student>());
        }

        [Fact]
        public async Task LastOrDefaultAsync_Empty_ReturnNull()
        {
            theDbContext.SetupDbSet(new List<Student>());

            var sut = GetSut();
            var res = await sut.LastOrDefaultAsync();

            res.Should().BeNull();
            theDbContext.Verify(x => x.Set<Student>());
        }

        [Theory, AutoData]
        public async Task ToArrayAsync_ReturnArray(List<Student> students)
        {
            theDbContext.SetupDbSet(students);

            var sut = GetSut();
            var res = await sut.ToArrayAsync();

            res.Should().BeEquivalentTo(students.ToArray());
            theDbContext.Verify(x => x.Set<Student>());
        }

        [Theory, AutoData]
        public async Task ToListAsync_ReturnList(List<Student> students)
        {
            theDbContext.SetupDbSet(students);

            var sut = GetSut();
            var res = await sut.ToListAsync();

            res.Should().BeEquivalentTo(students);
            theDbContext.Verify(x => x.Set<Student>());
        }

        [Fact]
        public void CreateUpdateBuilder_ReturnTheBuilder()
        {
            theDbContext.SetupDbSet(new List<Student>());

            var sut = GetSut();
            var res = sut.CreateUpdateBuilder();

            res.Should().BeOfType<EFUpdateSetBuilder<Student>>();
        }

        [Fact]
        public void ThePropertyOfQuery()
        {
            theDbContext.SetupDbSet(new List<Student>());

            var sut = GetSut();

            sut.Context.Should().BeEquivalentTo(theDbContext.Object);
            sut.ElementType.Should().Be(theDbContext.Object.Set<Student>().AsNoTracking().ElementType);
            sut.Expression.Should().Be(theDbContext.Object.Set<Student>().AsNoTracking().Expression);
            sut.Provider.Should().Be(theDbContext.Object.Set<Student>().AsNoTracking().Provider);
        }

        [Fact]
        public void QueryOfEfQuery()
        {
            var sut = GetMemorySut();

            var provider = sut.Context.GetService<IAsyncQueryProvider>();
            sut.CreateQuery(Expression.Empty()).Should().Be(provider.CreateQuery(Expression.Empty()));
        }

        [Theory, AutoData]
        public async Task UpdateInQuery_EntityWillUpdate(List<Student> students)
        {
            var hit = students[1].Hit;

            var sut = GetMemorySut();
            sut.Context.Set<Student>().AddRange(students);
            sut.Context.SaveChanges();
            sut.Context.ChangeTracker.Clear();
            var res = sut.Where(x => x.Name == students[1].Name).UpdateInQuery(x => x.Set(y => y.Hit, hit + 1));

            res.Should().Be(1);
            var data = await sut.Where(x => x.Name == students[1].Name).FirstOrDefaultAsync();
            data!.Hit.Should().Be(hit + 1);
        }

        [Theory, AutoData]
        public async Task UpdateInQueryAsync_EntityWillUpdate(List<Student> students)
        {
            var hit = students[1].Hit;

            var sut = GetMemorySut();
            sut.Context.Set<Student>().AddRange(students);
            sut.Context.SaveChanges();
            sut.Context.ChangeTracker.Clear();
            var res = await sut.Where(x => x.Name == students[1].Name).UpdateInQueryAsync(x => x.Set(y => y.Hit, hit + 1));

            res.Should().Be(1);
            var data = await sut.Where(x => x.Name == students[1].Name).FirstOrDefaultAsync();
            data!.Hit.Should().Be(hit + 1);
        }

        [Theory, AutoData]
        public async Task DeleteInQuery_EntityWillDelete(List<Student> students)
        {
            var hit = students[1].Hit;

            var sut = GetMemorySut();
            sut.Context.Set<Student>().AddRange(students);
            sut.Context.SaveChanges();
            sut.Context.ChangeTracker.Clear();
            var res = sut.Where(x => x.Name == students[1].Name).DeleteInQuery();

            res.Should().Be(1);
            var data = await sut.Where(x => x.Name == students[1].Name).FirstOrDefaultAsync();
            data.Should().BeNull();
        }

        [Theory, AutoData]
        public async Task DeleteInQueryAsync_EntityWillDelete(List<Student> students)
        {
            var hit = students[1].Hit;

            var sut = GetMemorySut();
            sut.Context.Set<Student>().AddRange(students);
            sut.Context.SaveChanges();
            sut.Context.ChangeTracker.Clear();
            var res = await sut.Where(x => x.Name == students[1].Name).DeleteInQueryAsync();

            res.Should().Be(1);
            var data = await sut.Where(x => x.Name == students[1].Name).FirstOrDefaultAsync();
            data.Should().BeNull();
        }

        private EFRespository<Student> GetSut() =>
            new EFRespository<Student>(theDbContext.Object);

        private EFRespository<Student> GetMemorySut()
        {
            var dbc = new TheDbContext(new DbContextOptionsBuilder<TheDbContext>().UseSqlite("DataSource=file::memory:?cache=shared").Options);
            dbc.Database.EnsureCreated();
            return new EFRespository<Student>(dbc);
        }

        [ExcludeFromCodeCoverage]
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
                    b.HasKey(x=>x.Name);
                });
            }
        }
    }
}
