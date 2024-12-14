namespace Sisusa.Data.Contracts
{

    /// <summary>
    /// Manages and executes a queue of write and read commands in a transactional context.
    /// </summary>
    public class DataCommandManager
    {
        private readonly Queue<IWriteCommand> _writeCommands = new();
        private readonly Queue<object> _readCommands = new();

        /// <summary>
        /// Adds a write command to the queue for later execution.
        /// </summary>
        /// <param name="command">The write command to queue.</param>
        public void QueueWriteCommand(IWriteCommand command)
        {
            ArgumentNullException.ThrowIfNull(command, nameof(command));
            _writeCommands.Enqueue(command);
        }

        /// <summary>
        /// Adds a read command to the queue for later execution.
        /// Validates that the command is either <see cref="IReadManyCommand{T}"/> or <see cref="IReadSingleCommand{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the data to be read.</typeparam>
        /// <param name="readCmd">The read command to queue.</param>
        /// <exception cref="ArgumentNullException">Thrown when the provided command is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the command is not a valid read command type.</exception>
        public void QueueReadCommand<T>(object readCmd) where T : class
        {
            ArgumentNullException.ThrowIfNull(readCmd, nameof(readCmd));
            if (readCmd is not IReadManyCommand<T> && readCmd is not IReadSingleCommand<T>)
                throw new ArgumentException(
                    "Queued command must be an implementation of a valid ReadCommand interface.",
                    nameof(readCmd)
                    );

            _readCommands.Enqueue(readCmd);
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
            using var transact = dbContext.BeginTransaction();
            try
            {
                while (_writeCommands.Count != 0)
                {
                    var writeCmd = _writeCommands.Dequeue();
                    await writeCmd.ExecuteAsync(dbContext);
                }
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
                while (_writeCommands.Count != 0)
                {
                    var writeCmd = _writeCommands.Dequeue();
                    writeCmd.Execute(dbContext);
                }
                transact.Commit();
            }
            catch
            {
                transact.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Executes all queued read commands synchronously within a database transaction and retrieves the results.
        /// </summary>
        /// <param name="dbContext">The database context to execute commands against.</param>
        /// <returns>A collection of objects read by the commands.</returns>
        /// <exception cref="Exception">Rethrows any exceptions encountered during execution.</exception>
        /// <exception cref="ArgumentNullException">If given a null reference for dbContext.</exception>
        public ICollection<object> TryExecuteReads(IDataSourceContext dbContext)
        {
            ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
            using var transact = dbContext.BeginTransaction();
            var results = new HashSet<object>();
            try
            {
                while (_readCommands.Count != 0)
                {
                    var cmd = _readCommands.Dequeue();
                    switch (cmd)
                    {
                        case IReadManyCommand<object> command:
                        {
                            var items = command.Execute(dbContext);
                            foreach (var item in items)
                            {
                                results.Add(item);
                            }

                            break;
                        }
                        case IReadSingleCommand<object> single:
                        {
                            var item = single.Execute(dbContext);
                            if (item != null)
                                results.Add(item);
                            break;
                        }
                    }
                }
                transact.Commit();
                return results;
            }
            catch
            {
                transact.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Executes all queued read commands asynchronously within a database transaction and retrieves the results.
        /// </summary>
        /// <param name="dataSourceContext">The database context to execute commands against.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a collection of objects read by the commands.</returns>
        /// <exception cref="Exception">Rethrows any exceptions encountered during execution.</exception>
        /// <exception cref="ArgumentNullException">If given a null reference to data context.</exception>
        public async Task<ICollection<object>> TryExecuteReadsAsync(IDataSourceContext dataSourceContext)
        {
            ArgumentNullException.ThrowIfNull(dataSourceContext, nameof(dataSourceContext));
            using var transact = dataSourceContext.BeginTransaction();
            var results = new HashSet<object>();
            try
            {
                while (_readCommands.Count != 0)
                {
                    var cmd = _readCommands.Dequeue();
                    switch (cmd)
                    {
                        case IReadManyCommand<object> command:
                        {
                            var items = await command.ExecuteAsync(dataSourceContext);
                            foreach (var item in items)
                            {
                                results.Add(item);
                            }

                            break;
                        }
                        case IReadSingleCommand<object> single:
                        {
                            var item = await single.ExecuteAsync(dataSourceContext);
                            if (item != null)
                                results.Add(item);
                            break;
                        }
                    }
                }
                transact.Commit();
                return results;
            }
            catch
            {
                transact.Rollback();
                throw;
            }
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
        public static async Task<IEnumerable<T>> TryExecuteReadsAsync<T>(IEnumerable<IReadCommand> readCommands, IDataSourceContext dbContext)
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
                        case IReadManyCommand<T> command:
                        {
                            var items = await command.ExecuteAsync(dbContext);
                            foreach (var item in items)
                            {
                                results.Add(item);
                            }

                            break;
                        }
                        case IReadSingleCommand<T> single:
                        {
                            var item = await single.ExecuteAsync(dbContext);
                            if (item != null)
                                results.Add(item);
                            break;
                        }
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

        /// <summary>
        /// Executes multiple read commands synchronously and retrieves the results.
        /// </summary>
        /// <typeparam name="T">The type of objects returned by the commands.</typeparam>
        /// <param name="readCommands">The read commands to execute.</param>
        /// <param name="dbContext">The database context to execute commands against.</param>
        /// <returns>A collection of objects read by the commands.</returns>
        /// <exception cref="Exception">Rethrows any exceptions encountered during execution.</exception>
        /// <exception cref="ArgumentNullException">If given null IEnumerable or IDataSourceContext instance.</exception>
        public static IEnumerable<T> TryExecuteReads<T>(IEnumerable<IReadCommand> readCommands, IDataSourceContext dbContext)
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
                        case IReadManyCommand<T> command:
                        {
                            var items = command.Execute(dbContext);
                            foreach (var item in items)
                            {
                                results.Add(item);
                            }

                            break;
                        }
                        case IReadSingleCommand<T> single:
                        {
                            var item = single.Execute(dbContext);
                            if (item != null)
                                results.Add(item);
                            break;
                        }
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
