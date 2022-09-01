namespace CQRS.Transactions
{
    using System.Data;

    /// <summary>
    /// An <see cref="IDbTransaction"/> decorator that completes the transaction
    /// only when the number of calls to <see cref="IDbTransaction.Commit"/> are equal
    /// to the number of calls to <see cref="IDbConnection.BeginTransaction()"/>.
    /// </summary>
    public class TransactionDecorator : IDbTransaction
    {
        private readonly ICompletionBehavior completionBehavior;
        private int transactionCount;
        private int commitCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionDecorator"/> class.
        /// </summary>
        /// <param name="dbConnection">The <see cref="IDbConnection"/> that started the transaction.</param>
        /// <param name="dbTransaction">The decorated <see cref="IDbTransaction"/>.</param>
        /// <param name="completionBehavior">The <see cref="ICompletionBehavior"/> that is responsible for completing the transaction.</param>
        public TransactionDecorator(IDbConnection dbConnection, IDbTransaction dbTransaction, ICompletionBehavior completionBehavior)
        {
            Connection = dbConnection;
            InnerDbTransaction = dbTransaction;
            this.completionBehavior = completionBehavior;
        }

        /// <summary>
        /// Gets the inner <see cref="IDbTransaction"/> being decorated.
        /// </summary>                        
        public IDbTransaction InnerDbTransaction { get; }

        /// <inheritdoc/>
        public IDbConnection Connection { get; }

        /// <inheritdoc/>
        public IsolationLevel IsolationLevel => InnerDbTransaction.IsolationLevel;

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        /// <inheritdoc/>
        public void Commit()
        {
            commitCount++;
            TryCompleteTransaction();
        }

        /// <inheritdoc/>
        public void Rollback()
        {
        }

        /// <summary>
        /// Called by the <see cref="ConnectionDecorator.BeginTransaction()"/>.
        /// </summary>
        internal void IncrementTransactionCount() => transactionCount++;

        internal void TryCompleteTransaction()
        {
            if (commitCount == transactionCount && completionBehavior is CommitCompletionBehavior)
            {
                completionBehavior.Complete(this);
            }
        }

        /// <summary>
        /// Completes the transaction by either performing a commit or a rollback.
        /// </summary>
        internal void CompleteTransaction()
        {
            if (commitCount == transactionCount)
            {
                completionBehavior.Complete(this);
            }
            else
            {
                InnerDbTransaction.Rollback();
            }

            InnerDbTransaction.Dispose();
        }
    }
}