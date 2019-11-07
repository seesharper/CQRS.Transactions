namespace CQRS.Transactions
{
    using System.Data;

    /// <summary>
    /// An <see cref="ICompletionBehavior"/> that performs a rollback
    /// instead of a committ.
    /// </summary>
    public class RollbackCompletionBehavior : ICompletionBehavior
    {
        /// <inheritdoc/>
        public void Complete(IDbTransaction dbTransaction)
        {
            dbTransaction.Rollback();
        }
    }
}