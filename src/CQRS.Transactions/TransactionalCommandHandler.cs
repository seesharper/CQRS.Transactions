using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using CQRS.Command.Abstractions;

namespace CQRS.Transactions
{
    public class TransactionalCommandHandler<TCommand> : ICommandHandler<TCommand>
    {
        private readonly IDbConnection dbConnection;
        private readonly ICommandHandler<TCommand> commandHandler;

        public TransactionalCommandHandler(IDbConnection dbConnection, ICommandHandler<TCommand> commandHandler)
        {
            this.dbConnection = dbConnection;
            this.commandHandler = commandHandler;
        }

        public async Task HandleAsync(TCommand command, CancellationToken cancellationToken = default)
        {
            using (var transaction = dbConnection.BeginTransaction())
            {
                await commandHandler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
                transaction.Commit();
            }
        }
    }
}
