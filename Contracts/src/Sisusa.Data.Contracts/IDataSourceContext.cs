using System.Data;

namespace Sisusa.Data.Contracts;

/// <summary>
/// Defines the contract for a data context that facilitates interactions with a data store.
/// </summary>
public interface IDataSourceContext : IDisposable, IAsyncDisposable
{

    /// <summary>
    /// Persists all changes made in the context to the data store asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous save operation.
    /// The task result contains the number of state entries written to the data store.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists all changes made in the context to the data store synchronously.
    /// </summary>
    /// <returns>the number of state entries written to the data store.</returns>
    int SaveChanges();

    /// <summary>
    /// Adds all the entities in the specified collection to the context.
    /// </summary>
    /// <param name="entities">The entities to add.</param>
    void AddAll(object[] entities);

    /// <summary>
    /// Adds all the entities in the specified collection to the context.
    /// </summary>
    /// <param name="entities">The entities to add.</param>
    void AddAll(IEnumerable<object> entities);

    /// <summary>
    /// Removes all the entities in the specified collection from the context.
    /// </summary>
    /// <param name="entities">The entities to remove or mark as removed.</param>
    void RemoveAll(object[] entities);

    /// <summary>
    /// Removes all the entities in the specified collection from the context.
    /// </summary>
    /// <param name="entities">The entities to remove or mark as removed.</param>
    void RemoveAll(IEnumerable<object> entities);

    /// <summary>
    /// Updates all the entities in the specified collection in the context.    
    /// </summary>
    /// <param name="entities">The entities to update or mark as updated.</param>
    void UpdateAll(object[] entities);

    /// <summary>
    /// Updates all the entities in the specified collection in the context.
    /// </summary>
    /// <param name="entities">The entities to update or mark as updated.</param>
    void UpdateAll(IEnumerable<object> entities);
    /// <summary>
    /// Retrieves a set of entities of the specified type from the data source context.
    /// </summary>
    /// <typeparam name="T">The type of the entities in the set.</typeparam>
    /// <returns>A <see cref="IEntityCollection{TEntity}"/> of the entities in the data source.</returns>
    //IEntityCollection<T> Entities<T>() where T : class;

}