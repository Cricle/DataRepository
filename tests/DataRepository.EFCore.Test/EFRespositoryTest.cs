using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Moq.EntityFrameworkCore;
using System.Collections;
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

            res.Should().Be(students[students.Count - 1]);
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
            var actual = sut.CreateQuery(sut.Context.Set<Student>().AsNoTracking().Expression);
            actual.Expression.Should().BeEquivalentTo(sut.Context.Set<Student>().AsNoTracking().Expression);
        }

        [Fact]
        public void QueryOfEfQueryWithEntity()
        {
            var sut = GetMemorySut();

            var provider = sut.Context.GetService<IAsyncQueryProvider>();
            var actual = sut.CreateQuery<Student>(sut.Context.Set<Student>().AsNoTracking().Expression);
            actual.Expression.Should().BeEquivalentTo(sut.Context.Set<Student>().AsNoTracking().Expression);
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

        [Theory, AutoData]
        public async Task BeginTransactionAsync_Rollback_NothingChanged(Student student)
        {
            var sut = GetMemorySut();
            sut.Context.Set<Student>().Add(student);
            sut.Context.SaveChanges();
            sut.Context.ChangeTracker.Clear();

            using (var trans = await sut.BeginTransactionAsync())
            {
                student.Hit++;
                sut.Context.SaveChanges();
                sut.Context.ChangeTracker.Clear();
                await trans.RollbackAsync();
            }
            sut.Context.Set<Student>().First().Hit.Should().Be(student.Hit - 1);
        }

        [Theory, AutoData]
        public async Task BeginTransactionAsync_Commit_NothingChanged(Student student)
        {
            var sut = GetMemorySut();
            sut.Context.Set<Student>().Add(student);
            sut.Context.SaveChanges();
            sut.Context.ChangeTracker.Clear();

            using (var trans = await sut.BeginTransactionAsync())
            {
                sut.Context.Set<Student>().ExecuteUpdate(x => x.SetProperty(y => y.Hit, y => y.Hit + 1));
                await trans.CommitAsync();
            }
            sut.Context.Set<Student>().Where(x=>x.Name==student.Name).First().Hit.Should().Be(student.Hit + 1);
        }

        [Theory, AutoData]
        public void Select_MustProjectColumn(Student student)
        {
            var sut = GetMemorySut();
            sut.Context.Set<Student>().Add(student);
            sut.Context.SaveChanges();
            sut.Context.ChangeTracker.Clear();
            var data = sut.Where(x=>x.Name==student.Name).Select(x => new
            {
                D1 = x.Hit + 1,
                N = x.Name
            }).First();

            data.Should().BeEquivalentTo(new
            {
                D1 = student.Hit + 1,
                N = student.Name
            });
        }

        [Theory, AutoData]
        public void GetEnumerator_EnumeratorFromDb(Student student)
        {
            theDbContext.Setup(x => x.Set<Student>()).ReturnsDbSet(new[] {student });

            var sut = GetSut();
            var enu = sut.GetEnumerator();

            enu.MoveNext().Should().BeTrue();
            enu.Current.Should().BeEquivalentTo(student);
            enu.MoveNext().Should().BeFalse();

            var enuAno = ((IEnumerable)sut).GetEnumerator();

            enuAno.MoveNext().Should().BeTrue();
            enuAno.Current.Should().BeEquivalentTo(student);
            enuAno.MoveNext().Should().BeFalse();
        }

        [Theory, AutoData]
        public async Task CountAsync_ReturnDataCount(List<Student> students)
        {
            theDbContext.Setup(x => x.Set<Student>()).ReturnsDbSet(students);

            var sut = GetSut();
            var res = await sut.CountAsync();

            res.Should().Be(students.Count);
        }

        [Theory, AutoData]
        public async Task FirstOrDefaultAsync_ReturnFirst(List<Student> students)
        {
            theDbContext.Setup(x => x.Set<Student>()).ReturnsDbSet(students);

            var sut = GetSut();
            var res = await sut.FirstOrDefaultAsync();

            res.Should().Be(students[0]);
        }

        [Theory, AutoData]
        public async Task FirstOrDefaultAsync_ReturnNullWhenNotFound(List<Student> students)
        {
            theDbContext.Setup(x => x.Set<Student>()).ReturnsDbSet(students);

            var sut = GetSut();
            var res = await sut.Where(x=>x.Hit==-1).FirstOrDefaultAsync();

            res.Should().BeNull();
        }

        [Theory, AutoData]
        public async Task LastOrDefaultAsync_ReturnFirst(List<Student> students)
        {
            theDbContext.Setup(x => x.Set<Student>()).ReturnsDbSet(students);

            var sut = GetSut();
            var res = await sut.LastOrDefaultAsync();

            res.Should().Be(students[students.Count-1]);
        }

        [Theory, AutoData]
        public async Task LastOrDefaultAsync_ReturnNullWhenNotFound(List<Student> students)
        {
            theDbContext.Setup(x => x.Set<Student>()).ReturnsDbSet(students);

            var sut = GetSut();
            var res = await sut.Where(x => x.Hit == -1).LastOrDefaultAsync();

            res.Should().BeNull();
        }

        [Theory, AutoData]
        public async Task AnyAsync_ReturnTrue(List<Student> students)
        {
            theDbContext.Setup(x => x.Set<Student>()).ReturnsDbSet(students);

            var sut = GetSut();
            var res = await sut.Where(x => x.Hit == students[0].Hit).AnyAsync();

            res.Should().BeTrue();
        }

        [Theory, AutoData]
        public async Task AnyAsync_ReturnFalse(List<Student> students)
        {
            theDbContext.Setup(x => x.Set<Student>()).ReturnsDbSet(students);

            var sut = GetSut();
            var res = await sut.Where(x => x.Hit == -1).AnyAsync();

            res.Should().BeFalse();
        }

        [Theory, AutoData]
        public async Task ToListAsync_ReturnDataList(List<Student> students)
        {
            theDbContext.Setup(x => x.Set<Student>()).ReturnsDbSet(students);

            var sut = GetSut();
            var res = await sut.ToListAsync();

            res.Should().BeEquivalentTo(students);
        }

        [Theory, AutoData]
        public async Task ToArrayAsync_ReturnDataArray(List<Student> students)
        {
            theDbContext.Setup(x => x.Set<Student>()).ReturnsDbSet(students);

            var sut = GetSut();
            var res = await sut.ToArrayAsync();

            res.Should().BeEquivalentTo(students.ToArray());
        }

        [Theory, AutoData]
        public async Task Take_ReturnDataTake(List<Student> students)
        {
            theDbContext.Setup(x => x.Set<Student>()).ReturnsDbSet(students);

            var sut = GetSut();
            var res = await sut.Take(1).ToArrayAsync();

            res.Should().BeEquivalentTo(students.Take(1).ToArray());
        }

        [Theory, AutoData]
        public async Task Skip_ReturnDataSkip(List<Student> students)
        {
            theDbContext.Setup(x => x.Set<Student>()).ReturnsDbSet(students);

            var sut = GetSut();
            var res = await sut.Skip(1).ToArrayAsync();

            res.Should().BeEquivalentTo(students.Skip(1).ToArray());
        }

        [Theory, AutoData]
        public async Task ByQuery_ReturnDataNewQuery(List<Student> students)
        {
            theDbContext.Setup(x => x.Set<Student>()).ReturnsDbSet(students);

            var sut = GetSut();
            var res = await sut.ByQuery(x=>x.Take(1)).ToArrayAsync();

            res.Should().BeEquivalentTo(students.Take(1).ToArray());
        }

        private EFRespository<Student> GetSut() =>
            new EFRespository<Student>(theDbContext.Object);

        private EFRespository<Student> GetMemorySut()
        {
            var dbc = new TheDbContext(new DbContextOptionsBuilder<TheDbContext>().UseSqlite("DataSource=file::memory:").Options);
            dbc.Database.EnsureDeleted();
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
                    b.HasKey(x => x.Name);
                });
            }
        }
    }
}
