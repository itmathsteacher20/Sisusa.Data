using System.Linq.Expressions;

namespace Sisusa.Data.Contracts
{
    /// <summary>
    /// Represents a collection of entities.
    /// 
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IEntityCollection<TEntity>  where TEntity : class
    {

        /// <summary>
        /// Finds a single item from the collection asynchronously.
        /// </summary>
        /// <param name="cancellationToken" > A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the single entity.</returns>
        public Task<TEntity> SingleAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Determines whether any elements in the collection satisfy a condition asynchronously.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition. If null, the method checks if the collection contains any elements.</param>
        /// <param name="cancellationToken" > A cancellation token to cancel the operation.</param> 
        /// <returns>A task that represents the asynchronous operation. The task result contains <c>true</c> if any elements match the condition; otherwise, <c>false</c>.</returns>
        public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Determines whether the collection contains any elements asynchronously.
        /// </summary>
        /// <param name="cancellationToken"> A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains <c>true</c> if the collection contains any elements; otherwise, <c>false</c>.</returns>
        public Task<bool> AnyAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Filters the collection based on a predicate asynchronously.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IQueryable{TEntity}"/> that contains elements from the collection that satisfy the condition.</returns>
        public IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Returns the first element of the collection that satisfies a condition asynchronously.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition. If null, the method returns the first element.</param>
        /// <param name="cancellationToken" > A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the first element that matches the condition.</returns>
        public Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the first element of the collection asynchronously.
        /// </summary>
        /// <param name="cancellationToken" > A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the first element in the collection.</returns>
        public Task<TEntity> FirstAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the first element of the collection that satisfies a condition or a default value if no such element is found asynchronously.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition. If null, the method returns the first element or default.</param>
        /// <param name="cancellationToken" > A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the first element that matches the condition, or the default value if no such element is found.</returns>
        public Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the first element of the collection or a default value if the collection is empty asynchronously.
        /// </summary>
        /// <param name="cancellationToken" > A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the first element, or the default value if the collection is empty.</returns>
        public Task<TEntity?> FirstOrDefaultAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns all elements in the collection as an enumerable asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="ICollection{TEntity}"/> of all elements in the collection.</returns>
        public IEnumerable<TEntity> AsEnumerable();

        /// <summary>
        /// Converts the collection to a list asynchronously.
        /// </summary>
        /// <param name="cancellationToken" > A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="List{TEntity}"/> of all elements in the collection.</returns>
        public Task<List<TEntity>> ToListAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Counts the number of elements in the collection asynchronously.
        /// </summary>
        /// <param name="cancellationToken"> A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of elements in the collection.</returns>
        public Task<int> CountAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Counts the number of elements in the collection.
        /// </summary>
        /// <returns>The number of elements in the collection.</returns>
        public int Count();

        /// <summary>
        /// Counts the number of elements in the collection as a long integer.
        /// </summary>
        /// <returns>The number of elements in the collection as a long integer.</returns>
        public long LongCount();

        /// <summary>
        /// Counts the number of elements in the collection asynchronously as a long integer.
        /// </summary>
        /// <param name="cancellationToken"> A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of elements in the collection as a long integer.</returns>
        public Task<long> LongCountAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds an entity to the collection asynchronously.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a range of entities to the collection asynchronously.
        /// </summary>
        /// <param name="entities">The entities to add.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes an entity from the collection.
        /// </summary>
        /// <param name="entity">The entity to remove.</param>
        public void Remove(TEntity entity);

        /// <summary>
        /// Removes an entity from the collection asynchronously.
        /// </summary>
        /// <param name="entity">The entity to remove.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a range of entities from the collection.
        /// </summary>
        /// <param name="entities">The entities to remove.</param>
        public void RemoveRange(IEnumerable<TEntity> entities);


        /// <summary>
        /// Asynchronously removes a range of entities from the collection
        /// </summary>
        /// <param name="entities">The entities to remove</param>
        /// <param name="cancellationToken" >Cancellation token to cancel the operation</param>
        /// <returns>Task representing the async operation.</returns>
        public Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an entity in the collection asynchronously.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds an entity in the collection by its key values asynchronously.
        /// </summary>
        /// <param name="keyValues">The key values of the entity to find.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the entity found, or <c>null</c> if no such entity is found.</returns>
        public Task<TEntity?> FindAsync(params object[] keyValues);
    }
}

