namespace CQRS.Transactions
{
    using System.Data;

    /// <summary>
    /// An IDbConnection decorator that ensures a single <see cref="IDbTransaction"/>.
    /// </summary>
    public class ConnectionDecorator : IDbConnection
    {
        private readonly ICompletionBehavior completionBehavior;
        private TransactionDecorator transactionDecorator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionDecorator"/> class.
        /// </summary>
        /// <param name="dbConnection">The IDbConnection to be decorated.</param>
        public ConnectionDecorator(IDbConnection dbConnection)
        : this(dbConnection, new CommitCompletionBehavior())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionDecorator"/> class.
        /// </summary>
        /// <param name="dbConnection">The IDbConnection to be decorated.</param>
        /// <param name="completionBehavior">The <see cref="ICompletionBehavior"/> that is responsible
        /// for committing the transaction.</param>
        public ConnectionDecorator(IDbConnection dbConnection, ICompletionBehavior completionBehavior)
        {
            InnerDbConnection = dbConnection;
            this.completionBehavior = completionBehavior;
        }

        /// <summary>
        /// Gets the inner <see cref="IDbConnection"/> being decorated.
        /// </summary>                        
        public IDbConnection InnerDbConnection { get; }

        /// <inheritdoc/>
        public string ConnectionString
        {
            get { return InnerDbConnection.ConnectionString; }
            set { InnerDbConnection.ConnectionString = value; }
        }

        /// <inheritdoc/>
        public int ConnectionTimeout => InnerDbConnection.ConnectionTimeout;

        /// <inheritdoc/>
        public string Database => InnerDbConnection.Database;

        /// <inheritdoc/>
        public ConnectionState State => InnerDbConnection.State;

        /// <inheritdoc/>
        public void Dispose()
        {
            transactionDecorator?.CompleteTransaction();
            InnerDbConnection.Dispose();
        }

        /// <inheritdoc/>
        public IDbTransaction BeginTransaction()
        {
            transactionDecorator ??= new TransactionDecorator(this, InnerDbConnection.BeginTransaction(), completionBehavior);
            transactionDecorator.IncrementTransactionCount();
            return transactionDecorator;
        }

        /// <inheritdoc/>
        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            transactionDecorator ??= new TransactionDecorator(this, InnerDbConnection.BeginTransaction(il), completionBehavior);
            transactionDecorator.IncrementTransactionCount();
            return transactionDecorator;
        }

        /// <inheritdoc/>
        public void Close() => InnerDbConnection.Close();

        /// <inheritdoc/>
        public void ChangeDatabase(string databaseName) => InnerDbConnection.ChangeDatabase(databaseName);

        /// <inheritdoc/>
        public IDbCommand CreateCommand() => InnerDbConnection.CreateCommand();

        /// <inheritdoc/>
        public void Open() => InnerDbConnection.Open();

        internal void OnTransactionCompleted(IDbTransaction transaction)
        {
            transactionDecorator = null;
            transaction.Dispose();
        }
    }
}