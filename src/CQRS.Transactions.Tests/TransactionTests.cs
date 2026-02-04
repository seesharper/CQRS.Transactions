using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Xunit;

namespace CQRS.Transactions.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void ShouldCommitTransaction()
        {
            var transactionMock = new Mock<DbTransaction>();
            var dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.Protected().Setup<DbTransaction>("BeginDbTransaction", IsolationLevel.Unspecified).Returns(transactionMock.Object);
            using (var connection = new DbConnectionDecorator(dbConnectionMock.Object))
            {
                var transaction = connection.BeginTransaction();
                transaction.Commit();
            }

            transactionMock.Verify(m => m.Commit(), Times.Once);
            transactionMock.Verify(m => m.Rollback(), Times.Never);
        }

        [Fact]
        public async Task ShouldCommitTransactionAsync()
        {
            var transactionMock = new Mock<DbTransaction>();
            var dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.Protected().Setup<ValueTask<DbTransaction>>("BeginDbTransactionAsync", IsolationLevel.Unspecified, CancellationToken.None).Returns(ValueTask.FromResult(transactionMock.Object));
            using (var connection = new DbConnectionDecorator(dbConnectionMock.Object))
            {
                var transaction = await connection.BeginTransactionAsync();
                await transaction.CommitAsync();
            }

            transactionMock.Verify(m => m.CommitAsync(CancellationToken.None), Times.Once);
            transactionMock.Verify(m => m.RollbackAsync(CancellationToken.None), Times.Never);
        }


        [Fact]
        public void ShouldRollbackTransaction()
        {
            var dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Open);
            var fakeTransaction = new FakeDbTransaction(dbConnectionMock.Object);
            dbConnectionMock.Protected().Setup<DbTransaction>("BeginDbTransaction", IsolationLevel.Unspecified).Returns(fakeTransaction);

            using (var connection = new DbConnectionDecorator(dbConnectionMock.Object))
            {
                var dbTransaction = connection.BeginTransaction();
                dbTransaction.Rollback();
            }

            fakeTransaction.RollbackCount.Should().Be(1);
            fakeTransaction.CommitCount.Should().Be(0);
        }

        [Fact]
        public async Task ShouldRollbackTransactionAsync()
        {
            var dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Open);
            var fakeTransaction = new FakeDbTransaction(dbConnectionMock.Object);
            dbConnectionMock.Protected().Setup<ValueTask<DbTransaction>>("BeginDbTransactionAsync", IsolationLevel.Unspecified, CancellationToken.None).Returns(ValueTask.FromResult<DbTransaction>(fakeTransaction));

            await using (var connection = new DbConnectionDecorator(dbConnectionMock.Object))
            {
                var dbTransaction = await connection.BeginTransactionAsync();
                await dbTransaction.RollbackAsync();
            }

            fakeTransaction.RollbackCount.Should().Be(1);
            fakeTransaction.CommitCount.Should().Be(0);
        }

        [Fact]
        public void ShouldRollbackTransactionWhenCommitIsNotCalled()
        {
            var dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Open);
            var fakeTransaction = new FakeDbTransaction(dbConnectionMock.Object);
            dbConnectionMock.Protected().Setup<DbTransaction>("BeginDbTransaction", IsolationLevel.Unspecified).Returns(fakeTransaction);

            using (var connection = new DbConnectionDecorator(dbConnectionMock.Object))
            {
                var dbTransaction = connection.BeginTransaction();
            }

            fakeTransaction.RollbackCount.Should().Be(1);
            fakeTransaction.CommitCount.Should().Be(0);
        }

        [Fact]
        public void ShouldRollbackTransactionUsingRollbackCompletionBehavior()
        {
            var dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Open);
            var fakeTransaction = new FakeDbTransaction(dbConnectionMock.Object);
            dbConnectionMock.Protected().Setup<DbTransaction>("BeginDbTransaction", IsolationLevel.Unspecified).Returns(fakeTransaction);

            using (var connection = new DbConnectionDecorator(dbConnectionMock.Object, new DbRollbackCompletionBehavior()))
            {
                var dbTransaction = connection.BeginTransaction();
                dbTransaction.Commit();
            }

            fakeTransaction.RollbackCount.Should().Be(1);
            fakeTransaction.CommitCount.Should().Be(0);
        }

        [Fact]
        public async Task ShouldRollbackTransactionAsyncUsingRollbackCompletionBehavior()
        {
            var dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Open);
            var fakeTransaction = new FakeDbTransaction(dbConnectionMock.Object);
            dbConnectionMock.Protected().Setup<ValueTask<DbTransaction>>("BeginDbTransactionAsync", IsolationLevel.Unspecified, CancellationToken.None).Returns(ValueTask.FromResult<DbTransaction>(fakeTransaction));

            await using (var connection = new DbConnectionDecorator(dbConnectionMock.Object, new DbRollbackCompletionBehavior()))
            {
                var dbTransaction = await connection.BeginTransactionAsync();
                await dbTransaction.CommitAsync();
            }

            fakeTransaction.RollbackCount.Should().Be(1);
            fakeTransaction.CommitCount.Should().Be(0);
        }


        [Fact]
        public void ShouldCommitWhenCommitCountIsEqualToTransactionCount()
        {
            var transactionMock = new Mock<DbTransaction>();
            var dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.Protected().Setup<DbTransaction>("BeginDbTransaction", IsolationLevel.Unspecified).Returns(transactionMock.Object);


            using (var connection = new DbConnectionDecorator(dbConnectionMock.Object, new DbCommitCompletionBehavior()))
            {
                using (var transaction = connection.BeginTransaction())
                {
                    transaction.Commit();
                }
                transactionMock.Verify(m => m.Commit(), Times.Once);
            }
        }

        [Fact]
        public async Task ShouldCommitAsyncWhenCommitCountIsEqualToTransactionCount()
        {
            var transactionMock = new Mock<DbTransaction>();
            var dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.Protected().Setup<ValueTask<DbTransaction>>("BeginDbTransactionAsync", IsolationLevel.Unspecified, CancellationToken.None).Returns(ValueTask.FromResult(transactionMock.Object));


            await using (var connection = new DbConnectionDecorator(dbConnectionMock.Object, new DbCommitCompletionBehavior()))
            {
                await using (var transaction = await connection.BeginTransactionAsync())
                {
                    await transaction.CommitAsync();
                }
                transactionMock.Verify(m => m.CommitAsync(CancellationToken.None), Times.Once);
            }
        }


        [Fact]
        public void ShouldHandleNestedBeginTransaction()
        {
            var transactionMock = new Mock<DbTransaction>();
            var dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.Protected().Setup<DbTransaction>("BeginDbTransaction", IsolationLevel.Unspecified).Returns(transactionMock.Object);

            using (var connection = new DbConnectionDecorator(dbConnectionMock.Object, new DbCommitCompletionBehavior()))
            {
                using (var outerTransaction = connection.BeginTransaction())
                {
                    using (var innerTransaction = connection.BeginTransaction())
                    {
                        innerTransaction.Commit();
                    }
                    outerTransaction.Commit();
                }
                transactionMock.Verify(m => m.Commit(), Times.Once);
            }
        }
    }
}
