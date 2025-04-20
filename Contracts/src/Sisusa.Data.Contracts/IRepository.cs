using System.Linq.Expressions;

namespace Sisusa.Data.Contracts
{
    /// <summary>
    /// Provides a contract for a repository that manages entities of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of entity the repository manages.</typeparam>
    /// <typeparam name="TId">The type of the identifier for the entity.</typeparam>
    public interface IRepository<T, in TId> where T : class
    {
        /// <summary>
        /// Find the record/entity that is uniquely identified by the given id.
        /// </summary>
        /// <param name="id">The unique identifier to search by.</param>
        /// <returns>If found, a valid record/entity otherwise null.</returns>
        Task<T?> FindByIdAsync(TId id);

        /// <summary>
        /// Checks whether the data store contains a record with the specified identifier.
        /// </summary>
        /// <param name="id">The key to check records against.</param>
        /// <returns>True if a record is found, false otherwise.</returns>
        Task<bool> HasByIdAsync(TId id);

        /// <summary>
        /// Finds all records in the datastore.
        /// Caution: may cause performance issues if used in large collections.
        /// </summary>
        /// <returns>A collection of all records in the datastore.</returns>
        Task<ICollection<T>> FindAllAsync();

        Task<ICollection<T>> FindAllByFilter(Expression<Func<T, bool>> filter);

        /// <summary>
        /// Counts the total number of records in the data store
        /// </summary>
        /// <returns>No. of records currently in the data store.</returns>
        Task<int> CountAsync();

        /// <summary>
        /// Counts the number of records/entities that match the specified filter criteria.
        /// </summary>
        /// <param name="filter">The criteria used to filter records/entities.</param>
        /// <returns>The count of records/entities that meet the filter criteria.</returns>
        Task<int> CountByFilterAsync(Expression<Func<T, bool>> filter);

        /// <summary>
        /// Updates an existing record/entity with the specified entity data.
        /// </summary>
        /// <param name="entity">The entity containing updated data to be saved.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task UpdateAsync(T entity);
        
        /// <summary>
        /// Updates the existing record/entity that is uniquely identified by the given id.
        /// </summary>
        /// <param name="id">Unique identifier of the record to find and update.</param>
        /// <param name="entity">The updated entity to push to the data store.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">If either id or entity are null.</exception>
        /// <exception cref="KeyNotFoundException">If there is no record with the given id in the data store.</exception>
        Task UpdateByIdAsync(TId id, T entity);

        /// <summary>
        /// Deletes the record/entity that is uniquely identified by the given id.
        /// </summary>
        /// <param name="id">The unique identifier of the record/entity to delete.</param>
        /// <returns>A task representing the asynchronous operation with no result.</returns>
        Task DeleteByIdAsync(TId id);

        /// <summary>
        /// Adds a new entity to the repository.
        /// </summary>
        /// <param name="entity">The entity to be added to the data store.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddNewAsync(T entity);

    }
}