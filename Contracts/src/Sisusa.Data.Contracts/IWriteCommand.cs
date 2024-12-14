namespace Sisusa.Data.Contracts
{


    /// <summary>
    /// Represents a write command that performs an operation on a data source context.
    /// </summary>
    public interface IWriteCommand
    {
        /// <summary>
        /// Executes the write command asynchronously against the specified data source context.
        /// </summary>
        /// <param name="context">The data source context where the operation will be performed.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task ExecuteAsync(IDataSourceContext context);

        /// <summary>
        /// Executes the write command synchronously against the specified data source context.
        /// </summary>
        /// <param name="context">The data source context where the operation will be performed.</param>
        public void Execute(IDataSourceContext context);
    }

}