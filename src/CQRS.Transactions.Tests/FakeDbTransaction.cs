using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace CQRS.Transactions.Tests
{
    internal sealed class FakeDbTransaction : DbTransaction
    {
        private readonly DbConnection _connection;

        public FakeDbTransaction(DbConnection connection, IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            _connection = connection;
            IsolationLevel = isolationLevel;
        }

        public int CommitCount { get; private set; }

        public int RollbackCount { get; private set; }

        public int DisposeCount { get; private set; }

        public int DisposeAsyncCount { get; private set; }

        public override IsolationLevel IsolationLevel { get; }

        protected override DbConnection DbConnection => _connection;

        public override void Commit()
            => CommitCount++;

        public override Task CommitAsync(CancellationToken cancellationToken = default)
        {
            CommitCount++;
            return Task.CompletedTask;
        }

        public override void Rollback()
            => RollbackCount++;

        public override Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            RollbackCount++;
            return Task.CompletedTask;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeCount++;
            }

            base.Dispose(disposing);
        }

        public override ValueTask DisposeAsync()
        {
            DisposeAsyncCount++;
            return base.DisposeAsync();
        }
    }
}