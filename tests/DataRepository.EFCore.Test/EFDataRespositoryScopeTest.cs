using Microsoft.EntityFrameworkCore;

namespace DataRepository.EFCore.Test
{
    public class EFDataRespositoryScopeTest
    {
        private readonly Mock<DbContext> dbContextMock = new();

        [Fact]
        public void GivenNullInit_MustThrow()
        {
            Assert.Throws<ArgumentNullException>(() => new EFDataRespositoryScope<DbContext>(null!));
        }

        [Fact]
        public void Create_MustCreateRespository()
        {
            dbContextMock.SetupDbSet(new List<object>());

            var sut = CreateSut();
            var rep = sut.Create<object>();

            rep.Should().NotBeNull();
            dbContextMock.VerifyOnce(x => x.Set<object>());
        }

        [Fact]
        public void Dispose_DbContextMustBeDisposed()
        {
            dbContextMock.SetupDbSet(new List<object>());

            var sut = CreateSut();
            sut.Dispose();

            dbContextMock.VerifyOnceNoOthersCall(x => x.Dispose());
        }

        private EFDataRespositoryScope<DbContext> CreateSut() => new EFDataRespositoryScope<DbContext>(dbContextMock.Object);
    }
}
