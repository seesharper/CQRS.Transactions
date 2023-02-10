using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Xunit;
using System.Threading;


namespace CQRS.Transactions.Tests
{
    public class DecoratorTests
    {

        [Fact]
        public void ShouldForwardOpenToDecoratedConnection()
        {
            var dbConnectionMock = new Mock<DbConnection>();

            using (var connection = new DbConnectionDecorator(dbConnectionMock.Object))
            {
                connection.Open();
            }

            dbConnectionMock.Verify(m => m.Open(), Times.Once);
        }

        [Fact]
        public async Task ShouldForwardOpenAsyncToDecoratedConnection()
        {
            var dbConnectionMock = new Mock<DbConnection>();
            using (var connection = new DbConnectionDecorator(dbConnectionMock.Object))
            {
                await connection.OpenAsync(CancellationToken.None);
            }

            dbConnectionMock.Verify(m => m.OpenAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void ShouldForwardChangeDatabaseToDecoratedConnection()
        {
            var dbConnectionMock = new Mock<DbConnection>();

            using (var connection = new DbConnectionDecorator(dbConnectionMock.Object))
            {
                connection.ChangeDatabase(string.Empty);
            }

            dbConnectionMock.Verify(m => m.ChangeDatabase(string.Empty), Times.Once);
        }

        [Fact]
        public void ShouldForwardChangeDatabaseAsyncToDecoratedConnection()
        {
            var dbConnectionMock = new Mock<DbConnection>();

            using (var connection = new DbConnectionDecorator(dbConnectionMock.Object))
            {
                connection.ChangeDatabaseAsync(string.Empty, CancellationToken.None);
            }

            dbConnectionMock.Verify(m => m.ChangeDatabaseAsync(string.Empty, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void ShouldForwardCloseToDecoratedConnection()
        {
            var dbConnectionMock = new Mock<DbConnection>();

            using (var connection = new DbConnectionDecorator(dbConnectionMock.Object))
            {
                connection.Close();
            }

            dbConnectionMock.Verify(m => m.Close(), Times.Once);
        }

        [Fact]
        public async Task ShouldForwardCloseAsyncToDecoratedConnection()
        {
            var dbConnectionMock = new Mock<DbConnection>();

            using (var connection = new DbConnectionDecorator(dbConnectionMock.Object))
            {
                await connection.CloseAsync();
            }

            dbConnectionMock.Verify(m => m.CloseAsync(), Times.Once);
        }

        [Fact]
        public void ShouldForwardCreateCommandToDecoratedConnection()
        {
            var dbConnectionMock = new Mock<DbConnection>();

            using (var connection = new DbConnectionDecorator(dbConnectionMock.Object))
            {
                connection.CreateCommand();
            }

            dbConnectionMock.Protected().Verify("CreateDbCommand", Times.Once());
        }

        [Fact]
        public void ShouldForwardEnlistTransactionToDecoratedConnection()
        {
            var dbConnectionMock = new Mock<DbConnection>();

            using (var connection = new DbConnectionDecorator(dbConnectionMock.Object))
            {
                connection.EnlistTransaction(System.Transactions.Transaction.Current);
            }

            dbConnectionMock.Verify(m => m.EnlistTransaction(null), Times.Once);
        }

        [Fact]
        public void ShouldForwardGetSchemaToDecoratedConnection()
        {
            var dbConnectionMock = new Mock<DbConnection>();

            dbConnectionMock.Setup(m => m.GetSchema()).Returns(new DataTable());

            using (var connection = new DbConnectionDecorator(dbConnectionMock.Object))
            {
                connection.GetSchema();
            }

            dbConnectionMock.Verify(m => m.GetSchema(), Times.Once);
        }

        [Fact]
        public void ShouldForwardGetSchemaWithCollectionNameToDecoratedConnection()
        {
            var dbConnectionMock = new Mock<DbConnection>();

            dbConnectionMock.Setup(m => m.GetSchema(It.IsAny<string>())).Returns(new DataTable());

            using (var connection = new DbConnectionDecorator(dbConnectionMock.Object))
            {
                connection.GetSchema(It.IsAny<string>());
            }

            dbConnectionMock.Verify(m => m.GetSchema(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void ShouldForwardGetSchemaWithCollectionNameAndRestrictValuesToDecoratedConnection()
        {
            var dbConnectionMock = new Mock<DbConnection>();

            dbConnectionMock.Setup(m => m.GetSchema(It.IsAny<string>(), It.IsAny<string[]>())).Returns(new DataTable());

            using (var connection = new DbConnectionDecorator(dbConnectionMock.Object))
            {
                connection.GetSchema(It.IsAny<string>(), It.IsAny<string[]>());
            }

            dbConnectionMock.Verify(m => m.GetSchema(It.IsAny<string>(), It.IsAny<string[]>()), Times.Once);
        }



        [Fact]
        public void ShouldForwardConnectionStringToDecoratedConnection()
        {
            var dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.SetupAllProperties();

            using (var connection = new DbConnectionDecorator(dbConnectionMock.Object))
            {
                connection.ConnectionString = TestData.ConnectionString;
                connection.ConnectionString.Should().Be(TestData.ConnectionString);
            }

            dbConnectionMock.VerifyGet(m => m.ConnectionString, Times.Once);
            dbConnectionMock.VerifySet(m => m.ConnectionString = TestData.ConnectionString, Times.Once);
        }

        [Fact]
        public void ShouldGetDataSourceFromDecoratedConnection()
        {
            var dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.SetupGet(m => m.DataSource).Returns(TestData.DataSource);
            using (var connection = new DbConnectionDecorator(dbConnectionMock.Object))
            {
                connection.DataSource.Should().Be(TestData.DataSource);
            }

            dbConnectionMock.VerifyGet(m => m.DataSource, Times.Once);

        }

        [Fact]
        public void ShouldGetServerVersionFromDecoratedConnection()
        {
            var dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.SetupGet(m => m.ServerVersion).Returns(TestData.ServerVersion);
            using (var connection = new DbConnectionDecorator(dbConnectionMock.Object))
            {
                connection.ServerVersion.Should().Be(TestData.ServerVersion);
            }

            dbConnectionMock.VerifyGet(m => m.ServerVersion, Times.Once);

        }

        [Fact]
        public void ShouldBeAbleToRaiseEvents()
        {
            var dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.SetupGet(m => m.ServerVersion).Returns(TestData.ServerVersion);
            dbConnectionMock.Setup(m => m.Open()).Raises(m => m.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));
            using (var connection = new DbConnectionDecorator(dbConnectionMock.Object))
            {
                ((bool)typeof(DbConnectionDecorator).GetProperty("CanRaiseEvents", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(connection)).Should().BeTrue();
                connection.StateChange += (s, a) =>
                {
                    a.CurrentState.Should().Be(ConnectionState.Open);
                };
                connection.Open();
            }
        }


        [Fact]
        public void ShouldForwardStateToDecoratedConnection()
        {
            var dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Open);

            using (var connection = new DbConnectionDecorator(dbConnectionMock.Object))
            {
                connection.State.Should().Be(ConnectionState.Open);

            }

            dbConnectionMock.VerifyGet(m => m.State, Times.Once);
        }

        [Fact]
        public void ShouldForwardDatabaseToDecoratedConnection()
        {
            var dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.SetupGet(m => m.Database).Returns(TestData.Database);

            using (var connection = new DbConnectionDecorator(dbConnectionMock.Object))
            {
                connection.Database.Should().Be(TestData.Database);
            }

            dbConnectionMock.VerifyGet(m => m.Database, Times.Once);
        }

        [Fact]
        public void ShouldForwardConnectionTimeoutToDecoratedConnection()
        {
            var dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.SetupGet(m => m.ConnectionTimeout).Returns(TestData.ConnectionTimeout);

            using (var connection = new DbConnectionDecorator(dbConnectionMock.Object))
            {
                connection.ConnectionTimeout.Should().Be(TestData.ConnectionTimeout);
            }

            dbConnectionMock.VerifyGet(m => m.ConnectionTimeout, Times.Once);
        }

        [Fact]
        public void ShouldForwardBeginTransactionToDecoratedConnection()
        {
            var transactionMock = new Mock<DbTransaction>();
            var dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.Protected().Setup<DbTransaction>("BeginDbTransaction", IsolationLevel.Unspecified).Returns(transactionMock.Object);

            using (var connection = new DbConnectionDecorator(dbConnectionMock.Object))
            {
                connection.BeginTransaction();
            }

            dbConnectionMock.Protected().Verify("BeginDbTransaction", Times.Once(), IsolationLevel.Unspecified);
        }

        [Fact]
        public void ShouldForwardBeginTransactionWithIsolationLevelToDecoratedConnection()
        {
            var transactionMock = new Mock<DbTransaction>();
            var dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.Protected().Setup<DbTransaction>("BeginDbTransaction", IsolationLevel.ReadCommitted).Returns(transactionMock.Object);

            using (var connection = new DbConnectionDecorator(dbConnectionMock.Object))
            {
                connection.BeginTransaction(IsolationLevel.ReadCommitted);
            }

            dbConnectionMock.Protected().Verify("BeginDbTransaction", Times.Once(), IsolationLevel.ReadCommitted);
        }


        [Fact]
        public async Task ShouldForwardBeginTransactionAsyncToDecoratedConnection()
        {
            var transactionMock = new Mock<DbTransaction>();
            var dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.Protected().Setup<ValueTask<DbTransaction>>("BeginDbTransactionAsync", IsolationLevel.Unspecified, CancellationToken.None).Returns(ValueTask.FromResult(transactionMock.Object));

            await using (var connection = new DbConnectionDecorator(dbConnectionMock.Object))
            {
                await connection.BeginTransactionAsync();
            }

            dbConnectionMock.Protected().Verify("BeginDbTransactionAsync", Times.Once(), IsolationLevel.Unspecified, CancellationToken.None);
        }

        [Fact]
        public async Task ShouldForwardBeginTransactionAsyncWithIsolationLevelToDecoratedConnection()
        {
            var transactionMock = new Mock<DbTransaction>();
            var dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.Protected().Setup<ValueTask<DbTransaction>>("BeginDbTransactionAsync", IsolationLevel.ReadCommitted, CancellationToken.None).Returns(ValueTask.FromResult(transactionMock.Object));

            await using (var connection = new DbConnectionDecorator(dbConnectionMock.Object))
            {
                await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted);
            }

            dbConnectionMock.Protected().Verify("BeginDbTransactionAsync", Times.Once(), IsolationLevel.ReadCommitted, CancellationToken.None);
        }


        [Fact]
        public void ShouldNotForwardDisposeToDecoratedTransaction()
        {
            var transactionMock = new Mock<DbTransaction>();
            var dbConnectionMock = new Mock<DbConnection>();
            using (var transaction = new DbTransactionDecorator(dbConnectionMock.Object, transactionMock.Object, new DbCommitCompletionBehavior()))
            {
            }

            transactionMock.Protected().Verify("Dispose", Times.Never(), args: true);
        }

        [Fact]
        public void ShouldGetConnectionFromTransaction()
        {
            var transactionMock = new Mock<DbTransaction>();
            var dbConnectionMock = new Mock<DbConnection>();

            using (var transaction = new DbTransactionDecorator(dbConnectionMock.Object, transactionMock.Object, new DbCommitCompletionBehavior()))
            {
                transaction.Connection.Should().BeAssignableTo<IDbConnection>();
            }
        }

        [Fact]
        public void ShouldGetIsolationLevel()
        {
            var transactionMock = new Mock<DbTransaction>();
            var dbConnectionMock = new Mock<DbConnection>();
            transactionMock.SetupGet(m => m.IsolationLevel).Returns(IsolationLevel.ReadCommitted);
            using (var transaction = new DbTransactionDecorator(dbConnectionMock.Object, transactionMock.Object, new DbCommitCompletionBehavior()))
            {
                transaction.IsolationLevel.Should().Be(IsolationLevel.ReadCommitted);
            }
        }


        [Fact]
        public void ShouldCommitTransactionWhenConnectionIsDisposed()
        {
            var dbConnectionMock = new Mock<DbConnection>();
            var transactionMock = new Mock<DbTransaction>();
            dbConnectionMock.Protected().Setup<DbTransaction>("BeginDbTransaction", IsolationLevel.Unspecified).Returns(transactionMock.Object);
            using (var connection = new DbConnectionDecorator(dbConnectionMock.Object))
            {
                var transaction = connection.BeginTransaction();
                transaction.Commit();
            }

            transactionMock.Verify(m => m.Commit(), Times.Once);
        }

        [Fact]
        public async Task ShouldCommitAsyncTransactionWhenConnectionIsDisposedAsync()
        {
            var dbConnectionMock = new Mock<DbConnection>();
            var transactionMock = new Mock<DbTransaction>();
            dbConnectionMock.Protected().Setup<ValueTask<DbTransaction>>("BeginDbTransactionAsync", IsolationLevel.Unspecified, CancellationToken.None).Returns(ValueTask.FromResult(transactionMock.Object));
            await using (var connection = new DbConnectionDecorator(dbConnectionMock.Object))
            {
                var transaction = await connection.BeginTransactionAsync();
                await transaction.CommitAsync();
            }

            transactionMock.Verify(m => m.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }


        [Fact]
        public void ShouldRollbackTransactionIfNotCommittedWhenConnectionIsDisposed()
        {
            var dbConnectionMock = new Mock<DbConnection>();
            var transactionMock = new Mock<DbTransaction>();
            var completionBehaviorMock = new Mock<IDbCompletionBehavior>();
            dbConnectionMock.Protected().Setup<DbTransaction>("BeginDbTransaction", IsolationLevel.Unspecified).Returns(transactionMock.Object);
            using (var connection = new DbConnectionDecorator(dbConnectionMock.Object, completionBehaviorMock.Object))
            {
                var transaction = connection.BeginTransaction();
            }

            transactionMock.Verify(m => m.Rollback(), Times.Once);
        }

        [Fact]
        public async Task ShouldRollbackTransactionAsyncIfNotCommittedWhenConnectionIsDisposed()
        {
            var dbConnectionMock = new Mock<DbConnection>();
            var transactionMock = new Mock<DbTransaction>();
            var completionBehaviorMock = new Mock<IDbCompletionBehavior>();
            dbConnectionMock.Protected().Setup<ValueTask<DbTransaction>>("BeginDbTransactionAsync", IsolationLevel.Unspecified, CancellationToken.None).Returns(ValueTask.FromResult(transactionMock.Object));
            await using (var connection = new DbConnectionDecorator(dbConnectionMock.Object, completionBehaviorMock.Object))
            {
                var transaction = connection.BeginTransactionAsync();
            }

            transactionMock.Verify(m => m.RollbackAsync(CancellationToken.None), Times.Once);
        }
    }
}
