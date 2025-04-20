using System.Linq.Expressions;

namespace Sisusa.Data.Contracts;

/// <summary>
/// Provides a contract for a repository that supports bulk operations on entities of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of entity the repository manages.</typeparam>
/// <typeparam name="TId">The type of the identifier for the entity.</typeparam>
public interface IRepositoryWithBulkOperations<T, in TId> : IRepository<T, TId> where T : class
{
    /// <summary>
    /// Adds multiple new entities to the data store.
    /// </summary>
    /// <param name="entities">The collection of entities to be added.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddManyAsync(IEnumerable<T> entities);

    /// <summary>
    /// Updates multiple entities in the data store.
    /// </summary>
    /// <param name="entities">A collection of entities to be updated.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateManyAsync(IEnumerable<T> entities);

    /// <summary>
    /// Updates the entities in the data store that match the specified filter with the provided update expression.
    /// </summary>
    /// <param name="filter">An expression to filter the entities to be updated.</param>
    /// <param name="updateExpression">An expression defining the updates to apply to the filtered entities.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateWhereAsync(Expression<Func<T, bool>> filter, Expression<Func<T, T>> updateExpression);

    /// <summary>
    /// Deletes entities from the data store that match the specified filter.
    /// </summary>
    /// <param name="filter">An expression used to filter the entities to be deleted.</param>
    /// <returns>A task representing the asynchronous operation, with a boolean indicating success or failure.</returns>
    Task DeleteWhereAsync(Expression<Func<T, bool>> filter);
}