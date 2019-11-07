namespace CQRS.Transactions
{
    using System.Data;

    /// <summary>
    /// An IDbConnection decorator that ensures a single <see cref="IDbTransaction"/>.
    /// </summary>
    public class ConnectionDecorator : IDbConnection
    {
        private readonly IDbConnection dbConnection;
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
            this.dbConnection = dbConnection;
            this.completionBehavior = completionBehavior;
        }

        /// <inheritdoc/>
        public string ConnectionString
        {
            get { return dbConnection.ConnectionString; }
            set { dbConnection.ConnectionString = value; }
        }

        /// <inheritdoc/>
        public int ConnectionTimeout => dbConnection.ConnectionTimeout;

        /// <inheritdoc/>
        public string Database => dbConnection.Database;

        /// <inheritdoc/>
        public ConnectionState State => dbConnection.State;

        /// <inheritdoc/>
        public void Dispose()
        {
            transactionDecorator?.CompleteTransaction();
            dbConnection.Dispose();
        }

        /// <inheritdoc/>
        public IDbTransaction BeginTransaction()
        {
            transactionDecorator ??= new TransactionDecorator(this, dbConnection.BeginTransaction(), completionBehavior);
            transactionDecorator.IncrementTransactionCount();
            return transactionDecorator;
        }

        /// <inheritdoc/>
        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            transactionDecorator ??= new TransactionDecorator(this, dbConnection.BeginTransaction(il), completionBehavior);
            transactionDecorator.IncrementTransactionCount();
            return transactionDecorator;
        }

        /// <inheritdoc/>
        public void Close() => dbConnection.Close();

        /// <inheritdoc/>
        public void ChangeDatabase(string databaseName) => dbConnection.ChangeDatabase(databaseName);

        /// <inheritdoc/>
        public IDbCommand CreateCommand() => dbConnection.CreateCommand();

        /// <inheritdoc/>
        public void Open() => dbConnection.Open();
    }
}