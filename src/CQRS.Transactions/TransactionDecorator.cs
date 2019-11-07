namespace CQRS.Transactions
{
    using System.Data;

    /// <summary>
    /// An <see cref="IDbTransaction"/> decorator that completes the transaction
    /// when the outermost transaction committed or rolled back.
    /// </summary>
    public class TransactionDecorator : IDbTransaction
    {
        private readonly IDbTransaction dbTransaction;
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
            this.dbTransaction = dbTransaction;
            this.completionBehavior = completionBehavior;
        }

        /// <inheritdoc/>
        public IDbConnection Connection { get; }

        /// <inheritdoc/>
        public IsolationLevel IsolationLevel => dbTransaction.IsolationLevel;

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        /// <inheritdoc/>
        public virtual void Commit() => commitCount++;

        /// <inheritdoc/>
        public void Rollback()
        {
        }

        /// <summary>
        /// Called by the <see cref="ConnectionDecorator.BeginTransaction()"/>.
        /// </summary>
        internal void IncrementTransactionCount() => transactionCount++;

        /// <summary>
        /// Completes the transaction by either performing a commit or a rollback.
        /// </summary>
        internal void CompleteTransaction()
        {
            if (commitCount == transactionCount)
            {
                completionBehavior.Complete(dbTransaction);
            }
            else
            {
                dbTransaction.Rollback();
            }

            dbTransaction.Dispose();
        }
    }
}