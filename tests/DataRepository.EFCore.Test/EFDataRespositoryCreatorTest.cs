using Microsoft.EntityFrameworkCore;

namespace DataRepository.EFCore.Test
{
    public class EFDataRespositoryCreatorTest
    {
        private readonly Mock<IDbContextFactory<DbContext>> dbContextFactory = new();
        private readonly Mock<DbContext> dbContextMock = new();

        [Fact]
        public void GivenNullInit_MustThrow()
        {
            Assert.Throws<ArgumentNullException>(() => new EFDataRespositoryCreator<DbContext>(null!));
        }

        [Fact]
        public void Create_MustReturnRespository()
        {
            dbContextMock.SetupDbSet(new List<object>());
            dbContextFactory.Setup(x => x.CreateDbContext()).Returns(dbContextMock.Object);

            var sut = GetSut();
            var rep = sut.Create<object>();

            rep.Should().NotBeNull();
            dbContextMock.VerifyOnce(x => x.Set<object>());
            dbContextFactory.VerifyOnce(x => x.CreateDbContext());
        }

        [Fact]
        public void CreateScope_MustReturnScope()
        {
            dbContextFactory.Setup(x => x.CreateDbContext()).Returns(dbContextMock.Object);

            var sut = GetSut();
            var rep = sut.CreateScope();

            rep.Should().BeOfType<EFDataRespositoryScope<DbContext>>();
            dbContextFactory.VerifyOnce(x => x.CreateDbContext());
        }

        private EFDataRespositoryCreator<DbContext> GetSut() => new EFDataRespositoryCreator<DbContext>(dbContextFactory.Object);
    }
}
