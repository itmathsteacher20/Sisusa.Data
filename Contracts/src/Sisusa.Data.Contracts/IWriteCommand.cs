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
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ExecuteAsync(IDataSourceContext dataSourceContext);
    }

}
