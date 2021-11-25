namespace CQRS.Transactions
{
    using System.Data;

    /// <summary>
    /// An <see cref="ICompletionBehavior"/> that performs a commit
    /// when the transaction completes.
    /// </summary>
    public class CommitCompletionBehavior : ICompletionBehavior
    {
        /// <inheritdoc/>
        public void Complete(TransactionDecorator dbTransaction)
        {
            dbTransaction.InnerDbTransaction.Commit();
            ((ConnectionDecorator)dbTransaction.Connection).OnTransactionCompleted(dbTransaction.InnerDbTransaction);
        }
    }
}