using DataRepository;
using DataRepository.Models;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace DataRepository.Test
{
    public class DataBatchRespositoryUpdateExtensionsTest
    {
        private readonly Mock<IDataBatchRespository<Student>> studentDataBatchRespositoryMock = new();
        private readonly Mock<IDataRespository<Student>> studentDataRespositoryMock = new();
        private readonly Mock<IUpdateSetBuilder<Student>> studentUpdateSetBuilderMock = new();

        [Fact]
        public void UpdateInQuery_Update()
        {
            var expression = Expression.Empty();
            studentDataBatchRespositoryMock.Setup(x => x.CreateUpdateBuilder()).Returns(studentUpdateSetBuilderMock.Object);
            studentDataBatchRespositoryMock.Setup(x => x.UpdateInQuery(expression)).Returns(1);
            studentUpdateSetBuilderMock.Setup(x => x.Build()).Returns(expression);
            var hit = false;

            var res = studentDataBatchRespositoryMock.Object.UpdateInQuery(b =>
            {
                b.Should().Be(studentUpdateSetBuilderMock.Object);
                hit = true;
            });

            res.Should().Be(1);
            hit.Should().BeTrue();
            studentUpdateSetBuilderMock.Verify(x => x.Build(), Times.Once());
            studentUpdateSetBuilderMock.VerifyNoOtherCalls();
            studentDataBatchRespositoryMock.Verify(x => x.CreateUpdateBuilder(), Times.Once());
            studentDataBatchRespositoryMock.Verify(x => x.UpdateInQuery(expression), Times.Once());
            studentDataBatchRespositoryMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task UpdateInQueryAsync_Update()
        {
            var expression = Expression.Empty();
            studentDataBatchRespositoryMock.Setup(x => x.CreateUpdateBuilder()).Returns(studentUpdateSetBuilderMock.Object);
            studentDataBatchRespositoryMock.Setup(x => x.UpdateInQueryAsync(expression, default)).ReturnsAsync(1);
            studentUpdateSetBuilderMock.Setup(x => x.Build()).Returns(expression);
            var hit = false;

            var res = await studentDataBatchRespositoryMock.Object.UpdateInQueryAsync(b =>
            {
                b.Should().Be(studentUpdateSetBuilderMock.Object);
                hit = true;
            });

            res.Should().Be(1);
            hit.Should().BeTrue();
            studentUpdateSetBuilderMock.Verify(x => x.Build(), Times.Once());
            studentUpdateSetBuilderMock.VerifyNoOtherCalls();
            studentDataBatchRespositoryMock.Verify(x => x.CreateUpdateBuilder(), Times.Once());
            studentDataBatchRespositoryMock.Verify(x => x.UpdateInQueryAsync(expression, default), Times.Once());
            studentDataBatchRespositoryMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(0,1)]
        [InlineData(-1,1)]
        [InlineData(1, 0)]
        [InlineData(1, -1)]
        public async Task PageQueryAsync_ReturnEmpty_WhenPageIndexOrPagetCountMin(int pageIndex,int pageCount)
        {
            var res = await DataBatchRespositoryUpdateExtensions.PageQueryAsync(studentDataRespositoryMock.Object, pageIndex, pageCount);

            res.Should().BeEquivalentTo(new WorkPageResult<Student>([], 0, pageIndex, 0));
            studentDataRespositoryMock.VerifyNoOtherCalls();
        }


        [Fact]
        public async Task PageQueryAsync_ReturnEmpty_WhenCountReturnZero()
        {
            var res = await DataBatchRespositoryUpdateExtensions.PageQueryAsync(studentDataRespositoryMock.Object, 1, 1);

            res.Should().BeEquivalentTo(new WorkPageResult<Student>([], 0, 1, 0));
            studentDataRespositoryMock.VerifyOnceNoOthersCall(x => x.CountAsync(default));
        }


        [Theory, AutoData]
        public async Task PageQueryAsync_ReturnPageOne(Student student)
        {
            var students = new List<Student>();
            for (int i = 0; i < 2; i++)
            {
                students.Add(student);
            }
            studentDataRespositoryMock.Setup(x => x.CountAsync(default)).ReturnsAsync(students.Count);
            studentDataRespositoryMock.Setup(x => x.Skip(0).Take(1).ToListAsync(default)).ReturnsAsync(new List<Student> { students[0] });

            var res = await DataBatchRespositoryUpdateExtensions.PageQueryAsync(studentDataRespositoryMock.Object, 1, 1);

            res.Should().BeEquivalentTo(new WorkPageResult<Student>([students[0]], 2, 1, 1));
            studentDataRespositoryMock.VerifyOnce(x => x.CountAsync(default));
            studentDataRespositoryMock.VerifyOnceNoOthersCall(x => x.Skip(0).Take(1).ToListAsync(default));
        }

        [Theory, AutoData]
        public async Task PageQueryAsync_ReturnPageTwo(Student student)
        {
            var students = new List<Student>();
            for (int i = 0; i < 2; i++)
            {
                students.Add(student);
            }
            studentDataRespositoryMock.Setup(x => x.CountAsync(default)).ReturnsAsync(students.Count);
            studentDataRespositoryMock.Setup(x => x.Skip(1).Take(1).ToListAsync(default)).ReturnsAsync(new List<Student> { students[1] });

            var res = await DataBatchRespositoryUpdateExtensions.PageQueryAsync(studentDataRespositoryMock.Object, 2, 1);

            res.Should().BeEquivalentTo(new WorkPageResult<Student>([students[1]], 2, 2, 1));
            studentDataRespositoryMock.VerifyOnce(x => x.CountAsync(default));
            studentDataRespositoryMock.VerifyOnceNoOthersCall(x => x.Skip(1).Take(1).ToListAsync(default));
        }

        [ExcludeFromCodeCoverage]
        public class Student
        {
            public int Id { get; set; }

            public string? Name { get; set; }
        }
    }
}
