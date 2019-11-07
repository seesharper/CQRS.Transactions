namespace CQRS.Transactions
{
    using System.Data;

    /// <summary>
    /// Represents the action to be taken when a transactions is completed/committed.
    /// </summary>
    public interface ICompletionBehavior
    {
        /// <summary>
        /// Called by the <see cref="TransactionDecorator"/> when a transaction is to be committed.
        /// </summary>
        /// <param name="dbTransaction">The <see cref="IDbTransaction"/> to be committed.</param>
        void Complete(IDbTransaction dbTransaction);
    }
}