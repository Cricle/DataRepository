using Microsoft.EntityFrameworkCore.Storage;

namespace DataRepository.EFCore.Test
{
    public class EFDataTransactionTest
    {
        private readonly Mock<IDbContextTransaction> dbContextTransactionMock = new Mock<IDbContextTransaction>();

        [Fact]
        public async Task CommitAsync_Commited()
        {
            dbContextTransactionMock.Setup(x => x.CommitAsync(default)).Returns(Task.CompletedTask);

            var sut = GetSut();
            await sut.CommitAsync();

            dbContextTransactionMock.Verify(x => x.CommitAsync(default), Times.Once());
            dbContextTransactionMock.VerifyNoOtherCalls();
            sut.Transaction.Should().Be(dbContextTransactionMock.Object);
            sut.TransactionId.Should().Be(dbContextTransactionMock.Object.TransactionId);
        }

        [Fact]
        public async Task RollbackAsync_Rollbacked()
        {
            dbContextTransactionMock.Setup(x => x.RollbackAsync(default)).Returns(Task.CompletedTask);

            var sut = GetSut();
            await sut.RollbackAsync();

            dbContextTransactionMock.Verify(x => x.RollbackAsync(default), Times.Once());
            dbContextTransactionMock.VerifyNoOtherCalls();
            sut.Transaction.Should().Be(dbContextTransactionMock.Object);
            sut.TransactionId.Should().Be(dbContextTransactionMock.Object.TransactionId);
        }

        private EFDataTransaction GetSut() => new EFDataTransaction(dbContextTransactionMock.Object);
    }
}
