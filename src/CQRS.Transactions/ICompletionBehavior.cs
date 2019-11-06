namespace CQRS.Transactions
{
    using System.Data;

    public interface ICompletionBehavior
    {
        void Complete(IDbTransaction dbTransaction);
    }
}