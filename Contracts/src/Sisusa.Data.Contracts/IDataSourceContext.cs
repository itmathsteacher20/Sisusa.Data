using System.Data;

namespace Sisusa.Data.Contracts;

/// <summary>
/// Defines the contract for a data context that facilitates interactions with a data store.
/// </summary>
public interface IDataSourceContext
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
    /// Retrieves a set of entities of the specified type from the data source context.
    /// </summary>
    /// <typeparam name="T">The type of the entities in the set.</typeparam>
    /// <returns>A <see cref="IEntityCollection{TEntity}"/> of the entities in the data source.</returns>
    //IEntityCollection<T> Entities<T>() where T : class;

}