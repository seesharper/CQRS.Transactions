namespace CQRS.Transactions
{
    using System.Data;

    public class CommitCompletionBehavior : ICompletionBehavior
    {
        public void Complete(IDbTransaction dbTransaction)
        {
            dbTransaction.Commit();
        }
    }
}