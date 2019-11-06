using System;
using System.Data;
using FluentAssertions;
using Moq;
using Xunit;

namespace CQRS.Transactions.Tests
{
    public class DecoratorTests
    {
        [Fact]
        public void ShouldForwardOpenToDecoratee()
        {
            var transactionMock = new Mock<IDbTransaction>();
            var dbConnectionMock = new Mock<IDbConnection>();

            using (var connection = new ConnectionDecorator(dbConnectionMock.Object))
            {
                connection.Open();
            }

            dbConnectionMock.Verify(m => m.Open(), Times.Once);
        }

        [Fact]
        public void ShouldForwardChangeDatabaseToDecoratee()
        {
            var transactionMock = new Mock<IDbTransaction>();
            var dbConnectionMock = new Mock<IDbConnection>();

            using (var connection = new ConnectionDecorator(dbConnectionMock.Object))
            {
                connection.ChangeDatabase(string.Empty);
            }

            dbConnectionMock.Verify(m => m.ChangeDatabase(string.Empty), Times.Once);
        }

        [Fact]
        public void ShouldForwardCloseToDecoratee()
        {
            var transactionMock = new Mock<IDbTransaction>();
            var dbConnectionMock = new Mock<IDbConnection>();

            using (var connection = new ConnectionDecorator(dbConnectionMock.Object))
            {
                connection.Close();
            }

            dbConnectionMock.Verify(m => m.Close(), Times.Once);
        }

        [Fact]
        public void ShouldForwardCreateCommandToDecoratee()
        {
            var transactionMock = new Mock<IDbTransaction>();
            var dbConnectionMock = new Mock<IDbConnection>();

            using (var connection = new ConnectionDecorator(dbConnectionMock.Object))
            {
                connection.CreateCommand();
            }

            dbConnectionMock.Verify(m => m.CreateCommand(), Times.Once);
        }

        [Fact]
        public void ShouldForwardConnectionStringToDecoratee()
        {
            var transactionMock = new Mock<IDbTransaction>();
            var dbConnectionMock = new Mock<IDbConnection>();
            dbConnectionMock.SetupAllProperties();

            using (var connection = new ConnectionDecorator(dbConnectionMock.Object))
            {
                connection.ConnectionString = TestData.ConnectionString;
                connection.ConnectionString.Should().Be(TestData.ConnectionString);
            }

            dbConnectionMock.VerifyGet(m => m.ConnectionString, Times.Once);
            dbConnectionMock.VerifySet(m => m.ConnectionString = TestData.ConnectionString, Times.Once);
        }

        [Fact]
        public void ShouldForwardStateToDecoratee()
        {
            var transactionMock = new Mock<IDbTransaction>();
            var dbConnectionMock = new Mock<IDbConnection>();
            dbConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Open);

            using (var connection = new ConnectionDecorator(dbConnectionMock.Object))
            {
                connection.State.Should().Be(ConnectionState.Open);

            }

            dbConnectionMock.VerifyGet(m => m.State, Times.Once);
        }

        [Fact]
        public void ShouldForwardDatabaseToDecoratee()
        {
            var transactionMock = new Mock<IDbTransaction>();
            var dbConnectionMock = new Mock<IDbConnection>();
            dbConnectionMock.SetupGet(m => m.Database).Returns(TestData.Database);

            using (var connection = new ConnectionDecorator(dbConnectionMock.Object))
            {
                connection.Database.Should().Be(TestData.Database);
            }

            dbConnectionMock.VerifyGet(m => m.Database, Times.Once);
        }

        [Fact]
        public void ShouldForwardConnectionTimeoutToDecoratee()
        {
            var transactionMock = new Mock<IDbTransaction>();
            var dbConnectionMock = new Mock<IDbConnection>();
            dbConnectionMock.SetupGet(m => m.ConnectionTimeout).Returns(TestData.ConnectionTimeout);

            using (var connection = new ConnectionDecorator(dbConnectionMock.Object))
            {
                connection.ConnectionTimeout.Should().Be(TestData.ConnectionTimeout);
            }

            dbConnectionMock.VerifyGet(m => m.ConnectionTimeout, Times.Once);
        }

        [Fact]
        public void ShouldForwardBeginTransactionToDecoratee()
        {
            var transactionMock = new Mock<IDbTransaction>();
            var dbConnectionMock = new Mock<IDbConnection>();
            dbConnectionMock.Setup(m => m.BeginTransaction()).Returns(transactionMock.Object);

            using (var connection = new ConnectionDecorator(dbConnectionMock.Object))
            {
                connection.BeginTransaction();
            }

            dbConnectionMock.Verify(m => m.BeginTransaction(), Times.Once);
        }

        [Fact]
        public void ShouldForwardBeginTransactionWithIsolationLevelToDecoratee()
        {
            var transactionMock = new Mock<IDbTransaction>();
            var dbConnectionMock = new Mock<IDbConnection>();
            dbConnectionMock.Setup(m => m.BeginTransaction(IsolationLevel.ReadCommitted)).Returns(transactionMock.Object);

            using (var connection = new ConnectionDecorator(dbConnectionMock.Object))
            {
                connection.BeginTransaction(IsolationLevel.ReadCommitted);
            }

            dbConnectionMock.Verify(m => m.BeginTransaction(IsolationLevel.ReadCommitted), Times.Once);
        }

        [Fact]
        public void ShouldNotForwardDisposeToDecoratee()
        {
            var transactionMock = new Mock<IDbTransaction>();
            var dbConnectionMock = new Mock<IDbConnection>();

            using (var transaction = new TransactionDecorator(dbConnectionMock.Object, transactionMock.Object, new CommitCompletionBehavior()))
            {
            }

            transactionMock.Verify(m => m.Dispose(), Times.Never);
        }

        [Fact]
        public void ShouldGetConnectionFromTransaction()
        {
            var transactionMock = new Mock<IDbTransaction>();
            var dbConnectionMock = new Mock<IDbConnection>();

            using (var transaction = new TransactionDecorator(dbConnectionMock.Object, transactionMock.Object, new CommitCompletionBehavior()))
            {
                transaction.Connection.Should().BeAssignableTo<IDbConnection>();
            }
        }

        [Fact]
        public void ShouldGetIsolationLevel()
        {
            var transactionMock = new Mock<IDbTransaction>();
            var dbConnectionMock = new Mock<IDbConnection>();
            transactionMock.SetupGet(m => m.IsolationLevel).Returns(IsolationLevel.ReadCommitted);
            using (var transaction = new TransactionDecorator(dbConnectionMock.Object, transactionMock.Object, new CommitCompletionBehavior()))
            {
                transaction.IsolationLevel.Should().Be(IsolationLevel.ReadCommitted);
            }
        }


        // [Fact]
        // public void ShouldForwardToDecoratee()
        // {
        //     var transactionMock = new Mock<IDbTransaction>();
        //     var dbConnectionMock = new Mock<IDbConnection>();
        //     dbConnectionMock.Setup(m => m.BeginTransaction()).Returns(transactionMock.Object);

        //     using (var connection = new ConnectionDecorator(dbConnectionMock.Object))
        //     {
        //         connection.Open();
        //         connection.ChangeDatabase(string.Empty);
        //         connection.Close();
        //         connection.CreateCommand();
        //         var transaction = connection.BeginTransaction(IsolationLevel.Unspecified);
        //         transaction.Connection.Should().BeOfType<ConnectionDecorator>();
        //         transaction.Dispose();

        //         connection.ConnectionString = "SomeConnectionString";
        //         var connectionString = connection.ConnectionString;
        //         var connectionTimeout = connection.ConnectionTimeout;
        //         var connectionState = connection.State;
        //         var database = connection.Database;
        //     }

        //     dbConnectionMock.Verify(m => m.Open(), Times.Once);
        //     dbConnectionMock.Verify(m => m.ChangeDatabase(string.Empty), Times.Once);
        //     dbConnectionMock.Verify(m => m.Close(), Times.Once);
        //     dbConnectionMock.Verify(m => m.CreateCommand(), Times.Once);
        //     dbConnectionMock.Verify(m => m.BeginTransaction(), Times.Once);
        //     dbConnectionMock.VerifyGet(m => m.ConnectionString, Times.Once);
        //     dbConnectionMock.VerifyGet(m => m.ConnectionTimeout, Times.Once);
        //     dbConnectionMock.VerifyGet(m => m.State, Times.Once);
        //     dbConnectionMock.VerifyGet(m => m.Database, Times.Once);
        //     dbConnectionMock.VerifySet(m => m.ConnectionString = "SomeConnectionString", Times.Once);
        // }
    }
}