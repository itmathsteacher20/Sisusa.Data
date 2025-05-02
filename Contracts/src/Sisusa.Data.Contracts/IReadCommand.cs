namespace Sisusa.Data.Contracts
{
    /// <summary>
    /// Represents a command that performs a read operation synchronously, returning an object.
    /// </summary>
    public interface IReadCommand<T> where T : class
    {
        /// <summary>
        /// Executes the read operation.
        /// </summary>
        /// <param name="dataSource">The data source against which to run the command.</param>
        /// <returns>The result of the read operation, which can be any object or null if no result is found.</returns>
        T? Execute(IDataSourceContext dataSource);
    }

    /// <summary>
    /// Represents a command that reads synchronously from the data source returning multiple records.
    /// </summary>
    /// <typeparam name="T">The Entity type to read from the data source.</typeparam>
    /// <typeparam name="TCollection">The collection type to load the objects into.</typeparam>
    public interface IReadCommand<T, TCollection> where TCollection:IEnumerable<T>
    {
        /// <summary>
        /// Executes the read operation.
        /// </summary>
        /// <param name="dataSource">The data source against which to execute the read.</param>
        /// <returns>Collection of read objects.</returns>
        TCollection Execute(IDataSourceContext dataSource);
    }

    /// <summary>
    /// Represents a command that reads asynchronously from the data source returning multiple records.
    /// </summary>
    /// <typeparam name="T">The entity type to read from the data source.</typeparam>
    /// <typeparam name="TCollection">The collection type to load matched objects into.</typeparam>
    public interface IReadAsyncCommand<T, TCollection> where TCollection:IEnumerable<T>
    {
        /// <summary>
        /// Executes the read operation.
        /// </summary>
        /// <param name="dataSource">The data source against which to execute the read.</param>
        /// <param name="cancellationToken"> A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>Collection of read objects(may be empty).</returns>
        TCollection ExecuteAsync(IDataSourceContext dataSource, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Represents a command that performs a read operation asynchronously, returning an object.
    /// </summary>
    public interface IReadAsyncCommand<T> where T : class 
    {
        /// <summary>
        /// Executes the read operation asynchronously.
        /// </summary>
        /// <param name="dataSourceContext">The data source from which to read.</param>
        /// <param name="cancellationToken"> A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation, returning the result of the read operation, or null if no result is found.</returns>
        Task<T?> ExecuteAsync(IDataSourceContext dataSourceContext, CancellationToken cancellationToken = default);
    }
}
