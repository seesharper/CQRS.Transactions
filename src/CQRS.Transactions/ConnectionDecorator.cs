namespace CQRS.Transactions
{
    using System.Data;

    public class ConnectionDecorator : IDbConnection
    {
        private readonly IDbConnection dbConnection;
        private readonly ICompletionBehavior completionBehavior;
        private TransactionDecorator transactionDecorator;

        public ConnectionDecorator(IDbConnection dbConnection)
        : this(dbConnection, new CommitCompletionBehavior())
        {
        }

        public ConnectionDecorator(IDbConnection dbConnection, ICompletionBehavior completionBehavior)
        {
            this.dbConnection = dbConnection;
            this.completionBehavior = completionBehavior;
        }

        public string ConnectionString
        {
            get { return dbConnection.ConnectionString; }
            set { dbConnection.ConnectionString = value; }
        }

        public int ConnectionTimeout => dbConnection.ConnectionTimeout;

        public string Database => dbConnection.Database;

        public ConnectionState State => dbConnection.State;

        public void Dispose()
        {
            transactionDecorator?.CompleteTransaction();
            dbConnection.Dispose();
        }

        public IDbTransaction BeginTransaction()
        {
            transactionDecorator ??= new TransactionDecorator(this, dbConnection.BeginTransaction(), completionBehavior);
            transactionDecorator.IncrementTransactionCount();
            return transactionDecorator;
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            transactionDecorator ??= new TransactionDecorator(this, dbConnection.BeginTransaction(il), completionBehavior);
            transactionDecorator.IncrementTransactionCount();
            return transactionDecorator;
        }

        public void Close() => dbConnection.Close();

        public void ChangeDatabase(string databaseName) => dbConnection.ChangeDatabase(databaseName);

        public IDbCommand CreateCommand() => dbConnection.CreateCommand();

        public void Open() => dbConnection.Open();
    }
}