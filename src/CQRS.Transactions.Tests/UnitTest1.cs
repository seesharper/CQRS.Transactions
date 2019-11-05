using System;
using System.Data;
using Moq;
using Xunit;

namespace CQRS.Transactions.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void ShouldCommitTransaction()
        {
            Mock<IDbTransaction> transactionMock = new Mock<IDbTransaction>();
            Mock<IDbConnection> dbConnectionMock = new Mock<IDbConnection>();
            dbConnectionMock.Setup(m => m.BeginTransaction()).Returns(transactionMock.Object);

            using (ConnectionDecorator connection = new ConnectionDecorator(dbConnectionMock.Object))
            {
                var transaction = connection.BeginTransaction();
                transaction.Commit();
            }

            transactionMock.Verify(m => m.Commit(), Times.Once);
            transactionMock.Verify(m => m.Rollback(), Times.Never);
        }

        [Fact]
        public void ShouldRollbackTransaction()
        {
            Mock<IDbTransaction> transactionMock = new Mock<IDbTransaction>();
            Mock<IDbConnection> dbConnectionMock = new Mock<IDbConnection>();
            dbConnectionMock.Setup(m => m.BeginTransaction()).Returns(transactionMock.Object);

            using (ConnectionDecorator connection = new ConnectionDecorator(dbConnectionMock.Object))
            {
                var transaction = connection.BeginTransaction();
                transaction.Rollback();
            }

            transactionMock.Verify(m => m.Rollback(), Times.Once);
            transactionMock.Verify(m => m.Commit(), Times.Never);
        }


    }
}
