using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using System.Data;

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

            dbContextTransactionMock.VerifyOnceNoOthersCall(x => x.CommitAsync(default));
            sut.Transaction.Should().Be(dbContextTransactionMock.Object);
            sut.TransactionId.Should().Be(dbContextTransactionMock.Object.TransactionId);
        }

        [Fact]
        public void Commit_Commited()
        {
            var sut = GetSut();
            sut.Commit();

            dbContextTransactionMock.VerifyOnceNoOthersCall(x => x.Commit());
            sut.Transaction.Should().Be(dbContextTransactionMock.Object);
            sut.TransactionId.Should().Be(dbContextTransactionMock.Object.TransactionId);
        }

        [Fact]
        public async Task RollbackAsync_Rollbacked()
        {
            dbContextTransactionMock.Setup(x => x.RollbackAsync(default)).Returns(Task.CompletedTask);

            var sut = GetSut();
            await sut.RollbackAsync();

            dbContextTransactionMock.VerifyOnceNoOthersCall(x => x.RollbackAsync(default));
            sut.Transaction.Should().Be(dbContextTransactionMock.Object);
            sut.TransactionId.Should().Be(dbContextTransactionMock.Object.TransactionId);
        }

        [Fact]
        public void Rollback_Rollbacked()
        {
            var sut = GetSut();
            sut.Rollback();

            dbContextTransactionMock.VerifyOnceNoOthersCall(x => x.Rollback());
            sut.Transaction.Should().Be(dbContextTransactionMock.Object);
            sut.TransactionId.Should().Be(dbContextTransactionMock.Object.TransactionId);
        }

        [Fact]
        public void Dispose_Disposed()
        {
            var sut = GetSut();
            sut.Dispose();

            ((IDbTransaction)sut).Connection.Should().BeNull();
            sut.IsolationLevel.Should().Be(IsolationLevel.ReadCommitted);
            dbContextTransactionMock.Verify(x => x.Dispose(), Times.Once());
            dbContextTransactionMock.VerifyNoOtherCalls();
            sut.Transaction.Should().Be(dbContextTransactionMock.Object);
            sut.TransactionId.Should().Be(dbContextTransactionMock.Object.TransactionId);
        }

        private EFDataTransaction GetSut(IsolationLevel level = IsolationLevel.ReadCommitted) => new EFDataTransaction(dbContextTransactionMock.Object, level);
    }
}
