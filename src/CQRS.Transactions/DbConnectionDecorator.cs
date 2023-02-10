using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace CQRS.Transactions;

/// <summary>
/// An <see cref="DbConnection"/> decorator that ensures a single <see cref="IDbTransaction"/>.
/// </summary>
public class DbConnectionDecorator : DbConnection
{
    private DbTransactionDecorator _transactionDecorator;

    private readonly IDbCompletionBehavior _completionBehavior;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbConnectionDecorator"/> class.
    /// </summary>
    /// <param name="dbConnection">The <see cref="DbConnection"/> being decorated
    /// for completing the transaction.</param>
    public DbConnectionDecorator(DbConnection dbConnection)
        : this(dbConnection, new DbCommitCompletionBehavior())
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="DbConnectionDecorator"/> class.
    /// </summary>
    /// <param name="dbConnection">The <see cref="DbConnection"/> being decorated.</param>
    /// <param name="completionBehavior">The <see cref="IDbCompletionBehavior"/> that is responsible
    /// for completing the transaction.</param>
    public DbConnectionDecorator(DbConnection dbConnection, IDbCompletionBehavior completionBehavior)
    {
        InnerDbConnection = dbConnection;
        _completionBehavior = completionBehavior;
        InnerDbConnection.StateChange += StateChangeHandler;
    }

    /// <inheritdoc />
    public override string ConnectionString { get => InnerDbConnection.ConnectionString; set => InnerDbConnection.ConnectionString = value; }

    /// <inheritdoc />
    public override int ConnectionTimeout => InnerDbConnection.ConnectionTimeout;

    /// <inheritdoc />
    public override string Database => InnerDbConnection.Database;

    /// <inheritdoc />
    public override string DataSource => InnerDbConnection.DataSource;

    /// <inheritdoc />
    public override string ServerVersion => InnerDbConnection.ServerVersion;

    /// <inheritdoc />
    public override ConnectionState State => InnerDbConnection.State;

    /// <inheritdoc />
    protected override bool CanRaiseEvents => true;

    /// <summary>
    /// Gets the inner <see cref="IDbConnection"/> being decorated.
    /// </summary>                        
    public DbConnection InnerDbConnection { get; }

    /// <inheritdoc />
    public override void ChangeDatabase(string databaseName)
        => InnerDbConnection.ChangeDatabase(databaseName);

    /// <inheritdoc />
    public override Task ChangeDatabaseAsync(string databaseName, CancellationToken cancellationToken = default)
        => InnerDbConnection.ChangeDatabaseAsync(databaseName, cancellationToken);

    /// <inheritdoc />
    public override void Close()
        => InnerDbConnection.Close();

    /// <inheritdoc />
    public override Task CloseAsync()
        => InnerDbConnection.CloseAsync();

    /// <inheritdoc />
    public override void EnlistTransaction(System.Transactions.Transaction transaction)
        => InnerDbConnection.EnlistTransaction(transaction);

    /// <inheritdoc />
    public override DataTable GetSchema()
        => InnerDbConnection.GetSchema();

    /// <inheritdoc />
    public override DataTable GetSchema(string collectionName)
        => InnerDbConnection.GetSchema(collectionName);

    /// <inheritdoc />
    public override DataTable GetSchema(string collectionName, string[] restrictionValues)
        => InnerDbConnection.GetSchema(collectionName, restrictionValues);

    /// <inheritdoc/>
    public override void Open()
        => InnerDbConnection.Open();

    /// <inheritdoc/>
    public override async Task OpenAsync(CancellationToken cancellationToken)
        => await InnerDbConnection.OpenAsync();

    /// <summary>
    /// Starts a new <see cref="DbTransaction"/> if this is the first call to <see cref="BeginDbTransaction"/>.
    /// If an existing transaction exists, this method will increment the transaction count.
    /// </summary>
    /// <param name="isolationLevel">Specifies the transaction locking behavior for the connection.</param>
    /// <returns>A new <see cref="DbTransaction"/> if this is the first call to <see cref="BeginDbTransaction"/>, otherwise the already started transaction.</returns>
    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
        _transactionDecorator ??= new DbTransactionDecorator(this, InnerDbConnection.BeginTransaction(isolationLevel), _completionBehavior);
        _transactionDecorator.IncrementTransactionCount();
        return _transactionDecorator;
    }

    /// <summary>
    /// Starts a new <see cref="DbTransaction"/> if this is the first call to <see cref="BeginDbTransactionAsync"/>.
    /// If an existing transaction exists, this method will increment the transaction count.
    /// </summary>
    /// <param name="isolationLevel">Specifies the transaction locking behavior for the connection.</param>
    /// <param name="cancellationToken">The cancellation instruction.</param>
    /// <returns>A new <see cref="DbTransaction"/> if this is the first call to <see cref="BeginDbTransactionAsync"/>, otherwise the already started transaction.</returns>
    protected override async ValueTask<DbTransaction> BeginDbTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken)
    {
        _transactionDecorator ??= new DbTransactionDecorator(this, await InnerDbConnection.BeginTransactionAsync(isolationLevel, cancellationToken), _completionBehavior);
        _transactionDecorator.IncrementTransactionCount();
        return _transactionDecorator;
    }

    /// <inheritdoc />
    protected override DbCommand CreateDbCommand() => InnerDbConnection.CreateCommand();

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_transactionDecorator != null)
            {
                _transactionDecorator.CompleteTransaction();
            }

            //_transactionDecorator?.CompleteTransaction();
            InnerDbConnection.StateChange -= StateChangeHandler;
            InnerDbConnection.Dispose();
        }

        //base.Dispose(disposing);
    }

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        if (_transactionDecorator != null)
        {
            await _transactionDecorator.CompleteTransactionAsync();
        }

        InnerDbConnection.StateChange -= StateChangeHandler;
        await InnerDbConnection.DisposeAsync();
        //await base.DisposeAsync();
    }

    internal void OnTransactionCompleted(DbTransaction dbTransaction)
    {
        _transactionDecorator = null;
        dbTransaction.Dispose();
    }

    internal async Task OnTransactionCompletedAsync(DbTransaction dbTransaction)
    {
        _transactionDecorator = null;
        await dbTransaction.DisposeAsync();
    }

    private void StateChangeHandler(object sender, StateChangeEventArgs stateChangeEventArguments)
    {
        OnStateChange(stateChangeEventArguments);
    }
}