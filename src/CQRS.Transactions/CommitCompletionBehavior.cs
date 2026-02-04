namespace CQRS.Transactions
{
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;



    /// <summary>
    /// An <see cref="IDbCompletionBehavior"/> that performs a commit
    /// when the transaction completes.
    /// </summary>
    public class DbCommitCompletionBehavior : IDbCompletionBehavior
    {
        /// <inheritdoc/>
        public void Complete(DbTransactionDecorator dbTransaction)
            => dbTransaction.InnerDbTransaction.Commit();

        /// <inheritdoc/>
        public async Task CompleteAsync(DbTransactionDecorator dbTransaction, CancellationToken cancellationToken = default)
        {
            await dbTransaction.InnerDbTransaction.CommitAsync(cancellationToken);
        }
    }
}