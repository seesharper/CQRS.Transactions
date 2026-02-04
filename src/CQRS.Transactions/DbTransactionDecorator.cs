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
    private int _isCompleted;

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
        if (!TryStartCompletion())
        {
            return;
        }

        if (_commitCount == _transactionCount)
        {
            _completionBehavior.Complete(this);
        }
        else
        {
            if (CanRollback())
            {
                InnerDbTransaction.Rollback();
            }
        }

        CompleteAndDispose();
    }

    /// <summary>
    /// Completes the transaction by either performing a commit or a rollback.
    /// </summary>
    internal async Task CompleteTransactionAsync()
    {
        if (!TryStartCompletion())
        {
            return;
        }

        if (_commitCount == _transactionCount)
        {
            await _completionBehavior.CompleteAsync(this);
        }
        else
        {
            if (CanRollback())
            {
                await InnerDbTransaction.RollbackAsync();
            }
        }

        await CompleteAndDisposeAsync();
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
        if (!TryStartCompletion())
        {
            return;
        }

        if (CanRollback())
        {
            InnerDbTransaction.Rollback();
        }

        CompleteAndDispose();
    }

    /// <inheritdoc />
    public override Task RollbackAsync(CancellationToken cancellationToken = default)
        => RollbackInternalAsync(cancellationToken);

    /// <summary>
    /// Called by the <see cref="DbConnectionDecorator.BeginDbTransaction(IsolationLevel)"/>.
    /// </summary>
    internal void IncrementTransactionCount() => _transactionCount++;

    internal void TryCompleteTransaction()
    {
        if (_commitCount == _transactionCount && _completionBehavior is DbCommitCompletionBehavior && TryStartCompletion())
        {
            _completionBehavior.Complete(this);
            CompleteAndDispose();
        }
    }

    internal async Task TryCompleteTransactionAsync(CancellationToken cancellationToken)
    {
        if (_commitCount == _transactionCount && _completionBehavior is DbCommitCompletionBehavior && TryStartCompletion())
        {
            await _completionBehavior.CompleteAsync(this, cancellationToken);
            await CompleteAndDisposeAsync();
        }
    }

    private async Task RollbackInternalAsync(CancellationToken cancellationToken)
    {
        if (!TryStartCompletion())
        {
            return;
        }

        if (CanRollback())
        {
            await InnerDbTransaction.RollbackAsync(cancellationToken);
        }

        await CompleteAndDisposeAsync();
    }

    private bool TryStartCompletion()
        => Interlocked.Exchange(ref _isCompleted, 1) == 0;

    private bool CanRollback()
    {
        var connection = InnerDbTransaction.Connection;
        return connection != null && connection.State != ConnectionState.Closed && connection.State != ConnectionState.Broken;
    }

    private void CompleteAndDispose()
    {
        var connectionDecorator = _dbConnectionDecorator as DbConnectionDecorator;
        if (connectionDecorator != null)
        {
            connectionDecorator.OnTransactionCompleted(InnerDbTransaction);
            return;
        }

        InnerDbTransaction.Dispose();
    }

    private async Task CompleteAndDisposeAsync()
    {
        var connectionDecorator = _dbConnectionDecorator as DbConnectionDecorator;
        if (connectionDecorator != null)
        {
            await connectionDecorator.OnTransactionCompletedAsync(InnerDbTransaction);
            return;
        }

        await InnerDbTransaction.DisposeAsync();
    }
}