using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
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
            var transactionMock = new Mock<DbTransaction>();
            var dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.Protected().Setup<DbTransaction>("BeginDbTransaction", IsolationLevel.Unspecified).Returns(transactionMock.Object);

            using (var connection = new DbConnectionDecorator(dbConnectionMock.Object))
            {
                var transaction = connection.BeginTransaction();
                transaction.Rollback();
            }

            transactionMock.Verify(m => m.Rollback(), Times.Once);
            transactionMock.Verify(m => m.Commit(), Times.Never);
        }

        [Fact]
        public async Task ShouldRollbackTransactionAsync()
        {
            var transactionMock = new Mock<DbTransaction>();
            var dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.Protected().Setup<ValueTask<DbTransaction>>("BeginDbTransactionAsync", IsolationLevel.Unspecified, CancellationToken.None).Returns(ValueTask.FromResult(transactionMock.Object));

            await using (var connection = new DbConnectionDecorator(dbConnectionMock.Object))
            {
                var transaction = await connection.BeginTransactionAsync();
                await transaction.RollbackAsync();
            }

            transactionMock.Verify(m => m.RollbackAsync(CancellationToken.None), Times.Once);
            transactionMock.Verify(m => m.CommitAsync(CancellationToken.None), Times.Never);
        }

        [Fact]
        public void ShouldRollbackTransactionWhenCommitIsNotCalled()
        {
            var transactionMock = new Mock<DbTransaction>();
            var dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.Protected().Setup<DbTransaction>("BeginDbTransaction", IsolationLevel.Unspecified).Returns(transactionMock.Object);

            using (var connection = new DbConnectionDecorator(dbConnectionMock.Object))
            {
                var transaction = connection.BeginTransaction();
            }

            transactionMock.Verify(m => m.Rollback(), Times.Once);
            transactionMock.Verify(m => m.Commit(), Times.Never);
        }

        [Fact]
        public void ShouldRollbackTransactionUsingRollbackCompletionBehavior()
        {
            var transactionMock = new Mock<DbTransaction>();
            var dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.Protected().Setup<DbTransaction>("BeginDbTransaction", IsolationLevel.Unspecified).Returns(transactionMock.Object);

            using (var connection = new DbConnectionDecorator(dbConnectionMock.Object, new DbRollbackCompletionBehavior()))
            {
                var transaction = connection.BeginTransaction();
                transaction.Commit();
            }

            transactionMock.Verify(m => m.Rollback(), Times.Once);
            transactionMock.Verify(m => m.Commit(), Times.Never);
        }

        [Fact]
        public async Task ShouldRollbackTransactionAsyncUsingRollbackCompletionBehavior()
        {
            var transactionMock = new Mock<DbTransaction>();
            var dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.Protected().Setup<ValueTask<DbTransaction>>("BeginDbTransactionAsync", IsolationLevel.Unspecified, CancellationToken.None).Returns(ValueTask.FromResult(transactionMock.Object));

            await using (var connection = new DbConnectionDecorator(dbConnectionMock.Object, new DbRollbackCompletionBehavior()))
            {
                var transaction = await connection.BeginTransactionAsync();
                await transaction.CommitAsync();
            }

            transactionMock.Verify(m => m.RollbackAsync(CancellationToken.None), Times.Once);
            transactionMock.Verify(m => m.CommitAsync(CancellationToken.None), Times.Never);
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
