namespace Sisusa.Data.Contracts
{
    /// <summary>
    /// Represents a command that performs a write operation synchronously.
    /// </summary>
    public interface IWriteCommand
    {
        /// <summary>
        /// Executes the write operation.
        /// </summary>
        void Execute(IDataSourceContext dataSource);
    }                                     

    /// <summary>
    /// Represents a command that performs a write operation asynchronously.
    /// </summary>
    public interface IWriteAsyncCommand
    {
        /// <summary>
        /// Executes the write operation asynchronously.
        /// </summary>
        /// <param name="dataSourceContext">The data source against which to execute the command.</param>
        /// <param name="cancellationToken"> A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ExecuteAsync(IDataSourceContext dataSourceContext, CancellationToken cancellationToken = default);
    }

}
