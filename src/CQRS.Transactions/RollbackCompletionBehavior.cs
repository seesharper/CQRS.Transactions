namespace CQRS.Transactions
{
    using System.Data;

    public class RollbackCompletionBehavior : ICompletionBehavior
    {
        public void Complete(IDbTransaction dbTransaction)
        {
            dbTransaction.Rollback();
        }
    }
}