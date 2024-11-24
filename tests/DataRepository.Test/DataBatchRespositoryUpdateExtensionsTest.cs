using DataRepository;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace DataRepository.Test
{
    public class DataBatchRespositoryUpdateExtensionsTest
    {
        private readonly Mock<IDataBatchRespository<Student>> studentDataBatchRespositoryMock = new();
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

        [ExcludeFromCodeCoverage]
        internal class Student
        {
            public int Id { get; set; }

            public string? Name { get; set; }
        }
    }
}
