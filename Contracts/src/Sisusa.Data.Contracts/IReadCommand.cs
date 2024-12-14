namespace Sisusa.Data.Contracts
{
    /// <summary>
    /// Represents a read command that retrieves data from a data source.
    /// </summary>
    public interface IReadCommand
    {
        /// <summary>
        /// Executes the read command asynchronously against the specified data source context.
        /// </summary>
        /// <param name="dbContext">The data source context used to execute the command.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains the object retrieved from the data source, or <c>null</c> if no data is found.
        /// </returns>
        Task<object?> ExecuteAsync(IDataSourceContext dbContext);

        /// <summary>
        /// Executes the read command synchronously against the specified data source context.
        /// </summary>
        /// <param name="dbContext">The data source context used to execute the command.</param>
        /// <returns>The object retrieved from the data source, or <c>null</c> if no data is found.</returns>
        object? Execute(IDataSourceContext dbContext);
    }

    /// <summary>
    /// Represents a read command that retrieves a single instance of type <typeparamref name="T"/> from a data source.
    /// </summary>
    /// <typeparam name="T">The type of the object to be retrieved by the command.</typeparam>
    public interface IReadSingleCommand<T> : IReadCommand
    {
        /// <summary>
        /// Executes the read command and retrieves a single instance of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="dbContext">The data source context used to execute the command.</param>
        /// <returns>An instance of type <typeparamref name="T"/>, or <c>null</c> if no data is found.</returns>
        new T? Execute(IDataSourceContext dbContext);

        /// <summary>
        /// Executes the read command asynchronously and retrieves a single instance of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="dbContext">The data source context used to execute the command.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains an instance of type <typeparamref name="T"/>, or <c>null</c> if no data is found.
        /// </returns>
        new Task<T?> ExecuteAsync(IDataSourceContext dbContext);
    }

    /// <summary>
    /// Represents a read command that retrieves multiple instances of type <typeparamref name="T"/> from a data source.
    /// </summary>
    /// <typeparam name="T">The type of the objects to be retrieved by the command.</typeparam>
    public interface IReadManyCommand<T> : IReadCommand
    {
        /// <summary>
        /// Executes the read command and retrieves multiple instances of type <typeparamref name="T"/> synchronously.
        /// </summary>
        /// <param name="dbContext">The data source context used to execute the command.</param>
        /// <returns>
        /// A collection of objects of type <typeparamref name="T"/> retrieved from the data source. 
        /// Returns an empty collection if no data is found.
        /// </returns>
        new ICollection<T> Execute(IDataSourceContext dbContext);

        /// <summary>
        /// Executes the read command asynchronously and retrieves multiple instances of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="dbContext">The data source context used to execute the command.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains a collection of objects of type <typeparamref name="T"/> retrieved from the data source. 
        /// Returns an empty collection if no data is found.
        /// </returns>
        new Task<ICollection<T>> ExecuteAsync(IDataSourceContext dbContext);
    }


}