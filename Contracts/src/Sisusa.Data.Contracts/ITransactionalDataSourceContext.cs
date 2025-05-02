using System.Data;

namespace Sisusa.Data.Contracts
{
    /// <summary>
    /// Extends <see cref="IDataSourceContext"/> for data sources that support transactions.
    /// </summary>
    public interface ITransactionalDataSourceContext : IDataSourceContext
    {
        /// <summary>
        /// Begins a transaction on the underlying data store.
        /// </summary>
        /// <returns>Reference to the created or opened <see cref="IDataTransaction"/></returns>
        IDataTransaction BeginTransaction();

        /// <summary>
        /// For providers that support the `ADO.NET` <see cref="IsolationLevel"/> enum.
        /// Begins a transaction on the underlying data store using the specified isolation level.
        /// </summary>
        /// <param name="isolationLevel">Desired level of isolation.</param>
        /// <returns>The opened <see cref="IDataTransaction"/>.</returns>
        IDataTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        /// <summary>
        /// Asynchronously begins a transaction using the provider's default isolation level (if required).
        /// </summary>
        /// <param name="cancellationToken">Token to observe cancellation requests.</param>
        /// <returns>Task that represents the asynchronous operation. The task result contains the <see cref="IDataTransaction"/></returns>
        Task<IDataTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously begins a transaction using the provided isolation level.
        /// </summary>
        /// <param name="isolationLevel">Desired level of isolation if supported by the provider.</param>
        /// <param name="cancellationToken">Token to observe cancellation requests.</param>
        /// <returns>Task that represents the asynchronous operation. Task result contains the <see cref="IDataTransaction"/></returns>
        Task<IDataTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken);
    }
}
