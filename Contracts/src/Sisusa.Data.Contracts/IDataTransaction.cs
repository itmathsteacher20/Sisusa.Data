namespace Sisusa.Data.Contracts
{

    /// <summary>
    /// Represents a transaction or unit-of-work boundary for data operations.
    /// Implementations should ensure that Dispose/DisposeAsync roll back if CommitAsync was not called successfully.
    /// </summary>
    public interface IDataTransaction : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Gets a unique identifier for this transaction instance. 
        /// Used for logging.
        /// </summary>
        Guid TransactionId { get; }

        /// <summary>
        /// Persists the changes made within the scope of this transaction to the data store asynchronously.
        /// </summary>
        /// <param name="cancellationToken">Token to observe while waiting for the task to complete</param>
        /// 
        /// <returns>A task representing the asynchronous save operation.</returns>
        Task CommitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Discards the changes made within the scope of this transaction to the data store.
        /// </summary>
        /// <param name="cancellationToken">Token to observe while waiting for this operation to complete.</param>
        /// <returns>A task representing the asynchronous rollback operation.</returns>
        Task RollbackAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Persists the changes made within the scope of this transaction to the data store synchronously.
        /// </summary>
        void Commit();

        /// <summary>
        /// Discards all changes made within the scope of this transaction.
        /// </summary>
        void Rollback();
    }
}
