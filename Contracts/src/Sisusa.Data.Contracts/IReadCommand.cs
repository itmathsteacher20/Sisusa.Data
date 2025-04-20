namespace Sisusa.Data.Contracts
{
    /// <summary>
    /// Represents a command that performs a read operation synchronously, returning an object.
    /// </summary>
    public interface IReadCommand
    {
        /// <summary>
        /// Executes the read operation.
        /// </summary>
        /// <returns>The result of the read operation, which can be any object or null if no result is found.</returns>
        object? Execute();
    }

    /// <summary>
    /// Represents a command that performs a read operation asynchronously, returning an object.
    /// </summary>
    public interface IReadAsyncCommand
    {
        /// <summary>
        /// Executes the read operation asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, returning the result of the read operation, or null if no result is found.</returns>
        Task<object?> ExecuteAsync();
    }
}
