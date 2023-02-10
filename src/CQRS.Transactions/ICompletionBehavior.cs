namespace CQRS.Transactions
{
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;



    /// <summary>
    /// Represents the action to be taken when a transactions is completed/committed.
    /// </summary>
    public interface IDbCompletionBehavior
    {
        /// <summary>
        /// Called by the <see cref="DbTransactionDecorator"/> when a transaction is to be completed.
        /// </summary>
        /// <param name="dbTransaction">The <see cref="DbTransactionDecorator"/> to be completed.</param>
        void Complete(DbTransactionDecorator dbTransaction);

        /// <summary>
        /// Called by the <see cref="DbTransactionDecorator"/> when a transaction is to be completed.
        /// </summary>
        /// <param name="dbTransaction">The <see cref="DbTransactionDecorator"/> to be completed.</param>
        /// <param name="cancellationToken">An optional token to cancel the asynchronous operation. The default value is <see cref="CancellationToken.None"/> .</param>
        Task CompleteAsync(DbTransactionDecorator dbTransaction, CancellationToken cancellationToken = default);
    }
}