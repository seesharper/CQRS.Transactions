namespace CQRS.Transactions
{
    using System.Data;

    public class TransactionDecorator : IDbTransaction
    {
        private readonly IDbTransaction dbTransaction;
        private readonly ICompletionBehavior completionBehavior;
        private int transactionCount;
        private int commitCount;

        public TransactionDecorator(IDbConnection dbConnection, IDbTransaction dbTransaction, ICompletionBehavior completionBehavior)
        {
            Connection = dbConnection;
            this.dbTransaction = dbTransaction;
            this.completionBehavior = completionBehavior;
        }

        public IDbConnection Connection { get; }

        public IsolationLevel IsolationLevel => dbTransaction.IsolationLevel;

        public void IncrementTransactionCount() => transactionCount++;

        public void CompleteTransaction()
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

        public void Dispose()
        {
        }

        public virtual void Commit() => commitCount++;

        public void Rollback()
        {
        }
    }
}