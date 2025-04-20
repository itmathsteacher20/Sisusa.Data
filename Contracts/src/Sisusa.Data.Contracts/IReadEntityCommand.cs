namespace Sisusa.Data.Contracts
{

    /// <summary>
    /// Represents a command that retrieves a single entity synchronously.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be retrieved.</typeparam>
    public interface IReadSingleEntityCommand<TEntity> : IReadCommand where TEntity : class
    {
        /// <summary>
        /// Executes the read operation, returning a single entity.
        /// </summary>
        /// <returns>The entity of type <typeparamref name="TEntity"/>, or null if not found.</returns>
        new TEntity? Execute();
    }

    /// <summary>
    /// Represents a command that retrieves a single entity asynchronously.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be retrieved.</typeparam>
    public interface IReadSingleEntityAsyncCommand<TEntity> : IReadAsyncCommand where TEntity : class
    {
        /// <summary>
        /// Executes the read operation asynchronously, returning a single entity.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, returning the entity of type <typeparamref name="TEntity"/>, or null if not found.</returns>
        new Task<TEntity?> ExecuteAsync();
    }

    /// <summary>
    /// Represents a command that retrieves multiple entities asynchronously.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities to be retrieved.</typeparam>
    public interface IReadManyEntitiesAsyncCommand<TEntity> : IReadAsyncCommand where TEntity : class
    {
        /// <summary>
        /// Executes the read operation asynchronously, returning multiple entities.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, returning a collection of entities of type <typeparamref name="TEntity"/>.</returns>
        new Task<IEnumerable<TEntity>> ExecuteAsync();
    }

    /// <summary>
    /// Represents a command that retrieves multiple entities synchronously.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities to be retrieved.</typeparam>
    public interface IReadManyEntitiesCommand<TEntity> : IReadCommand where TEntity : class
    {
        /// <summary>
        /// Executes the read operation, returning multiple entities.
        /// </summary>
        /// <returns>A collection of entities of type <typeparamref name="TEntity"/>.</returns>
        new IEnumerable<TEntity> Execute();
    }
}
