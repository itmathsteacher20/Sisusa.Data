using System.Linq.Expressions;

namespace Sisusa.Data.Contracts;

/// <summary>
/// Extends the repository contract to include support for retrieving paginated collections of entities
/// of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of entity the repository manages.</typeparam>
/// <typeparam name="TId">The type of the identifier for the entity.</typeparam>
public interface IPaginatedRepository<T, in TId> : IRepository<T, TId> where T : class
{
    /// <summary>
    /// Retrieves a collection of all records with support for pagination.
    /// </summary>
    /// <param name="page">The current page number to retrieve.</param>
    /// <param name="pageSize">The number of records per page.</param>
    /// <returns>A collection of records for the specified page.</returns>
    Task<ICollection<T>> FindAllWithPagingAsync(int page, int pageSize);
}



/// <summary>
/// Defines a repository interface with functionality to search for entities based on a search term.
/// </summary>
/// <typeparam name="T">The type of the entity managed by the repository.</typeparam>
/// <typeparam name="TId">The type of the entity's unique identifier.</typeparam>
public interface ISearchableRepository<T, in TId> : IRepository<T, TId> where T : class
{
    /// <summary>
    /// Searches for entities that match the specified search term and satisfy the given predicate.
    /// </summary>
    /// <param name="searchTerm">The term to search for in the entities.</param>
    /// <param name="predicate">
    /// A lambda expression used to filter the entities. 
    /// This is evaluated in combination with the search term.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a collection 
    /// of entities that match the search term and satisfy the predicate.
    /// </returns>
    Task<ICollection<T>> SearchAsync(string searchTerm, Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Searches for entities that match the specified search term within the specified properties.
    /// </summary>
    /// <param name="searchTerm">The term to search for in the entities.</param>
    /// <param name="propertiesToSearch">
    /// An array of property names to include in the search. 
    /// Only these properties will be searched for the given term.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a collection 
    /// of entities that match the search term within the specified properties.
    /// </returns>
    Task<ICollection<T>> SearchAsync(string searchTerm, string[] propertiesToSearch);
}
