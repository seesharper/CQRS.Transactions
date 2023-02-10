using System.Threading;
using System.Threading.Tasks;
namespace CQRS.Transactions
{
    /// <summary>
    /// An <see cref="IDbCompletionBehavior"/> that performs a rollback
    /// when the transaction completes.
    /// </summary>
    public class DbRollbackCompletionBehavior : IDbCompletionBehavior
    {
        /// <inheritdoc/>    
        public void Complete(DbTransactionDecorator dbTransaction)
            => dbTransaction.InnerDbTransaction.Rollback();

        /// <inheritdoc/>
        public async Task CompleteAsync(DbTransactionDecorator dbTransaction, CancellationToken cancellationToken = default)
            => await dbTransaction.InnerDbTransaction.RollbackAsync(cancellationToken);
    }
}