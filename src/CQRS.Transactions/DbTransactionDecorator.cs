using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace CQRS.Transactions;

/// <summary>
/// An <see cref="DbTransaction"/> decorator that completes the transaction
/// only when the number of calls to <see cref="DbTransaction.Commit"/> or <see cref="DbTransaction.CommitAsync"/> are equal
/// to the number of calls to <see cref="DbConnection.BeginTransaction()"/> or <see cref="DbConnection.BeginTransactionAsync(CancellationToken)"/>
/// </summary>
public class DbTransactionDecorator : DbTransaction
{
    private readonly DbConnection _dbConnectionDecorator;
    private readonly IDbCompletionBehavior _completionBehavior;
    private int _transactionCount;
    private int _commitCount;

    /// <summary>
    /// Initializes a new instance of the DbTransactionDecorator class.
    /// </summary>
    /// <param name="dbConnectionDecorator">The <see cref="DbConnectionDecorator"/> that started the transaction.</param>
    /// <param name="dbTransaction">The decorated <see cref="DbTransaction"/>.</param>
    /// <param name="completionBehavior">The <see cref="IDbCompletionBehavior"/> that is responsible for completing the transaction.</param>
    public DbTransactionDecorator(DbConnection dbConnectionDecorator, DbTransaction dbTransaction, IDbCompletionBehavior completionBehavior)
    {
        _dbConnectionDecorator = dbConnectionDecorator;
        InnerDbTransaction = dbTransaction;
        _completionBehavior = completionBehavior;
    }

    /// <summary>
    /// Gets the inner <see cref="IDbTransaction"/> being decorated.
    /// </summary>                        
    public DbTransaction InnerDbTransaction { get; }


    /// <summary>
    /// Completes the transaction by either performing a commit or a rollback.
    /// </summary>
    internal void CompleteTransaction()
    {
        if (_commitCount == _transactionCount)
        {
            _completionBehavior.Complete(this);
        }
        else
        {
            InnerDbTransaction.Rollback();
        }

        InnerDbTransaction.Dispose();
    }

    /// <summary>
    /// Completes the transaction by either performing a commit or a rollback.
    /// </summary>
    internal async Task CompleteTransactionAsync()
    {
        if (_commitCount == _transactionCount)
        {
            await _completionBehavior.CompleteAsync(this);
        }
        else
        {
            await InnerDbTransaction.RollbackAsync();
        }

        await InnerDbTransaction.DisposeAsync();
    }

    /// <inheritdoc />
    public override IsolationLevel IsolationLevel => InnerDbTransaction.IsolationLevel;

    /// <inheritdoc />
    protected override DbConnection DbConnection => _dbConnectionDecorator;


    /// <inheritdoc/>
    public override void Commit()
    {
        _commitCount++;
        TryCompleteTransaction();
    }

    /// <inheritdoc/>
    public override async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        _commitCount++;
        await TryCompleteTransactionAsync(cancellationToken);
    }


    /// <inheritdoc />
    public override void Rollback()
    {
    }

    /// <inheritdoc />
    public override Task RollbackAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    /// Called by the <see cref="DbConnectionDecorator.BeginDbTransaction(IsolationLevel)"/>.
    /// </summary>
    internal void IncrementTransactionCount() => _transactionCount++;

    internal void TryCompleteTransaction()
    {
        if (_commitCount == _transactionCount && _completionBehavior is DbCommitCompletionBehavior)
        {
            _completionBehavior.Complete(this);
        }
    }

    internal async Task TryCompleteTransactionAsync(CancellationToken cancellationToken)
    {
        if (_commitCount == _transactionCount && _completionBehavior is DbCommitCompletionBehavior)
        {
            await _completionBehavior.CompleteAsync(this, cancellationToken);
        }
    }
}