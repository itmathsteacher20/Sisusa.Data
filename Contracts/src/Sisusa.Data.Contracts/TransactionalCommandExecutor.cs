namespace Sisusa.Data.Contracts
{

    /// <summary>
    /// Manages and executes a queue of write  commands in a transactional context.
    /// </summary>
    public class TransactionalCommandExecutor
    {
        private readonly List<IWriteAsyncCommand> _asyncWrites = [];
        private readonly List<IWriteCommand> _writes = [];

        private bool _hasAsyncCommandsQueued = false;
        private bool _hasSyncCommandsQueued = false;

        private void Cleanup()
        {
            _asyncWrites.Clear();
            _writes.Clear();
            _hasAsyncCommandsQueued = false;
            _hasSyncCommandsQueued = false;
        }


        //private readonly List<IReadAsyncCommand> _asyncReadCommands = [];
        //private readonly List<IReadCommand> _readSyncCommands = [];

        /// <summary>
        /// Adds a write command to the queue for later execution.
        /// </summary>
        /// <param name="command">The write command to queue.</param>
        /// <exception cref="InvalidOperationException">If this method is called after adding asynchronous write commands.</exception>
        public void QueueWriteCommand(IWriteCommand command)
        {
            ArgumentNullException.ThrowIfNull(command, nameof(command));
            if (_hasAsyncCommandsQueued)
            {
                throw new InvalidOperationException("Cannot queue synchronous write command with async commands.");
            }
                        
            _writes.Add(command);

            if (!_hasSyncCommandsQueued)
                _hasSyncCommandsQueued = true;
        }

        /// <summary>
        /// Adds an asynchronous write command to the batch 
        /// </summary>
        /// <param name="writeAsyncCommand">The async command to add to the batch.</param>
        /// <exception cref="InvalidOperationException">If synchronous and async commands are being mixed</exception>
        public void QueueWriteAsyncCommand(IWriteAsyncCommand writeAsyncCommand)
        {
            ArgumentNullException.ThrowIfNull(writeAsyncCommand, nameof(writeAsyncCommand));
            if  (_hasSyncCommandsQueued)
            {
                throw new InvalidOperationException("Cannot mix synchronous write commands with asynchronous ones.");
            }
            _asyncWrites.Add(writeAsyncCommand);
            if (!_hasAsyncCommandsQueued)
                _hasAsyncCommandsQueued = true;
        }

        /// <summary>
        /// Adds a read command to the queue for later execution.
        /// Validates that the command is either <see cref="IReadManyCommand{T}"/> or <see cref="IReadSingleCommand{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the data to be read.</typeparam>
        /// <param name="readCmd">The read command to queue.</param>
        /// <exception cref="ArgumentNullException">Thrown when the provided command is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the command is not a valid read command type.</exception>
        //public void QueueReadCommand<T>(IReadCommand readCmd) where T : class
        //{
        //    ArgumentNullException.ThrowIfNull(readCmd, nameof(readCmd));

        //    _readSyncCommands.Add(readCmd);
        //}

        //public void QueueAsyncReadCommand<T>(IReadAsyncCommand readCmd) where T : class
        //{
        //    ArgumentNullException.ThrowIfNull(readCmd);
        //    _asyncReadCommands.Add(readCmd);
        //}


        ///<summary>
        /// Executes all asynchronous write operations
        ///</summary>
        private async Task PerformWritesAsync(IDataSourceContext dbContext, CancellationToken cancellationToken = default)
        {
            if (_writes.Count != 0)
            {
                throw new InvalidOperationException(
                    $"Cannot run synchronous writes inside the async call. Please the `{nameof(TryExecuteWrites)}` method."
                    );
            }
            foreach (var cmd in _asyncWrites)
            {
                await cmd.ExecuteAsync(dbContext, cancellationToken);
            }
        }

        private void PerformWrites(IDataSourceContext dbContext)
        {
            if (_asyncWrites.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Cannot perform asynchronous writes inside the synchronous call. Please use the `{nameof(TryExecuteWritesAsync)}` method."
                    );
            }
            foreach (var write in _writes)
            {
                write.Execute(dbContext);

            }
        }

        /// <summary>
        /// Executes all queued write commands asynchronously within a database transaction.
        /// </summary>
        /// <param name="dbContext">The database context to execute commands against.</param>
        /// <exception cref="Exception">Rethrows any exceptions encountered during execution.</exception>
        /// <param name="cancellationToken">Token to observe cancellation requests.</param>
        /// <exception cref="ArgumentNullException">If given a null reference to dbContext.</exception>
        public async Task TryExecuteWritesAsync(ITransactionalDataSourceContext dbContext, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
            using var transact = await dbContext.BeginTransactionAsync(cancellationToken);
            try
            {
                await PerformWritesAsync(dbContext, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken); //
                transact.Commit();
            }
            catch
            {
                transact.Rollback();
                throw;
            }
            finally
            {
                Cleanup();
            }
        }

        /// <summary>
        /// Executes all queued write commands synchronously within a database transaction.
        /// </summary>
        /// <param name="dbContext">The database context to execute commands against.</param>
        /// <exception cref="Exception">Rethrows any exceptions encountered during execution.</exception>
        /// <exception cref="ArgumentNullException">If given a null dbContext.</exception>
        public void TryExecuteWrites(ITransactionalDataSourceContext dbContext)
        {
            ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
            using var transact = dbContext.BeginTransaction();
            try
            {
                PerformWrites(dbContext);
                dbContext.SaveChanges();
                transact.Commit();
            }
            catch
            {
                transact.Rollback();
                throw;
            }
            finally
            {
                Cleanup();
            }
        }
    }
}
