namespace Sisusa.Data.Contracts
{

    /// <summary>
    /// Manages and executes a queue of write and read commands in a transactional context.
    /// </summary>
    public class DataCommandManager
    {
        //private readonly Queue<IWriteCommand> _writeCommands = new();
       // private readonly Queue<IReadCommand> _readCommands = new();

        private readonly List<object> _writeCommands = [];
        private readonly List<object> _readCommands = [];

        /// <summary>
        /// Adds a write command to the queue for later execution.
        /// </summary>
        /// <param name="command">The write command to queue.</param>
        public void QueueWriteCommand(IWriteCommand command)
        {
            ArgumentNullException.ThrowIfNull(command, nameof(command));
            if (command is not IWriteCommand && command is not IWriteAsyncCommand)
                throw new ArgumentException("Argument should be instance of the Write commands.", nameof(command));
            _writeCommands.Add(command);
        }

        public void QueueWriteAsyncCommand(IWriteAsyncCommand writeAsyncCommand)
        {
            ArgumentNullException.ThrowIfNull(writeAsyncCommand, nameof(writeAsyncCommand));

            _writeCommands.Add(writeAsyncCommand);
        }

        /// <summary>
        /// Adds a read command to the queue for later execution.
        /// Validates that the command is either <see cref="IReadManyCommand{T}"/> or <see cref="IReadSingleCommand{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the data to be read.</typeparam>
        /// <param name="readCmd">The read command to queue.</param>
        /// <exception cref="ArgumentNullException">Thrown when the provided command is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the command is not a valid read command type.</exception>
        public void QueueReadCommand<T>(IReadCommand readCmd) where T : class
        {
            ArgumentNullException.ThrowIfNull(readCmd, nameof(readCmd));

            _readCommands.Add(readCmd);
        }

        public void QueueAsyncReadCommand<T>(IReadAsyncCommand readCmd) where T : class
        {
            ArgumentNullException.ThrowIfNull(readCmd);
            _readCommands.Add(readCmd);
        }

        private async void PerformWrites()
        {
            foreach (var command in _writeCommands)
            {
                if (command is IWriteAsyncCommand asyncCmd)
                {
                    await asyncCmd.ExecuteAsync();
                }
                if (command is IWriteCommand writeCmd)
                {
                    writeCmd.Execute();
                }

            }
        }

        /// <summary>
        /// Executes all queued write commands asynchronously within a database transaction.
        /// </summary>
        /// <param name="dbContext">The database context to execute commands against.</param>
        /// <exception cref="Exception">Rethrows any exceptions encountered during execution.</exception>
        /// <exception cref="ArgumentNullException">If given a null reference to dbContext.</exception>
        public async Task TryExecuteWritesAsync(IDataSourceContext dbContext)
        {
            ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
            using var transact = await dbContext.BeginTransactionAsync();
            try
            {
                PerformWrites();
                transact.Commit();
            }
            catch
            {
                transact.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Executes all queued write commands synchronously within a database transaction.
        /// </summary>
        /// <param name="dbContext">The database context to execute commands against.</param>
        /// <exception cref="Exception">Rethrows any exceptions encountered during execution.</exception>
        /// <exception cref="ArgumentNullException">If given a null dbContext.</exception>
        public void TryExecuteWrites(IDataSourceContext dbContext)
        {
            ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
            using var transact = dbContext.BeginTransaction();
            try
            {
                PerformWrites();
                transact.Commit();
            }
            catch
            {
                transact.Rollback();
                throw;
            }
        }


        private async Task<IEnumerable<object>> PerformReads()
        {
            var results = new HashSet<object>();

            foreach (var cmd in _readCommands)
            {
                switch (cmd)
                {
                    case IReadSingleEntityCommand<object> readOne:
                        var obj = readOne.Execute();
                        if (obj != null)
                            results.Add(obj);
                        break;
                    case IReadSingleEntityAsyncCommand<object> readOneAsync:
                        obj = await readOneAsync.ExecuteAsync();
                        if (obj != null)
                            results.Add(obj);
                        break;
                    case IReadManyEntitiesCommand<object> readMany:
                        var objs = readMany.Execute();
                        foreach (var item in objs)
                        {
                            results.Add(item);
                        }
                        break;
                    case IReadManyEntitiesAsyncCommand<object> readManyAsync:
                        objs = await readManyAsync.ExecuteAsync();
                        foreach (var item in objs)
                        {
                            results.Add(item);
                        }
                        break;
                    default:
                        continue;
                }
            }
            return results;
        }

        /// <summary>
        /// Executes all queued read commands asynchronously within a database transaction and retrieves the results.
        /// </summary>
        /// <param name="dataSourceContext">The database context to execute commands against.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a collection of objects read by the commands.</returns>
        /// <exception cref="Exception">Rethrows any exceptions encountered during execution.</exception>
        /// <exception cref="ArgumentNullException">If given a null reference to data context.</exception>
        public async Task<IEnumerable<T>> TryExecuteReadsAsync<T>(IDataSourceContext dataSourceContext) where T : class
        {
            ArgumentNullException.ThrowIfNull(dataSourceContext, nameof(dataSourceContext));
            return await TryExecuteReadsAsync<T>(_readCommands.Select(rc=>(IReadCommand)rc).ToList(), dataSourceContext);
        }

        /// <summary>
        /// Executes multiple read commands asynchronously and retrieves the results.
        /// </summary>
        /// <typeparam name="T">The type of objects returned by the commands.</typeparam>
        /// <param name="readCommands">The read commands to execute.</param>
        /// <param name="dbContext">The database context to execute commands against.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a collection of objects read by the commands.</returns>
        /// <exception cref="Exception">Rethrows any exceptions encountered during execution.</exception>
        /// <exception cref="ArgumentNullException">If given null reference to readCommands or DataSourceContext.</exception>
        public static async Task<IEnumerable<T>> TryExecuteReadsAsync<T>(ICollection<IReadCommand> readCommands, IDataSourceContext dbContext) where T: class
        {
            ArgumentNullException.ThrowIfNull(readCommands, nameof(readCommands));
            ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
            
            using var transaction = dbContext.BeginTransaction();
            var results = new HashSet<T>();
            try
            {
                foreach (var readCommand in readCommands)
                {
                    switch (readCommand)
                    {
                        case IReadManyEntitiesCommand<T> command:
                        {
                            var items = command.Execute();
                            foreach (var item in items)
                            {
                                results.Add(item);
                            }

                            break;
                        }
                        case IReadManyEntitiesAsyncCommand<T> asyncCmd:
                            {
                                var items = await asyncCmd.ExecuteAsync();
                                foreach (var item in items)
                                {
                                    results.Add(item);
                                }
                                break;
                            }
                        case IReadSingleEntityCommand<T> single:
                        {
                            var item = single.Execute();
                            if (item != null)
                                results.Add(item);
                            break;
                        }
                        case IReadSingleEntityAsyncCommand<T> singleAsync:
                            {
                                var item = await singleAsync.ExecuteAsync();
                                if (item != null)
                                    results.Add(item);

                                break;
                            }
                        default:
                            continue; //skip bad commands
                    }
                }
                transaction.Commit();
                return results;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
