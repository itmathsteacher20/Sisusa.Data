using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Sisusa.Data.Contracts;
using Microsoft.EntityFrameworkCore.Query;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Sisusa.Data.EFCore;

/// <summary>
/// Represents ordering information for a query result, including the key, whether it is primary, and the ordering mode.
/// </summary>
/// <typeparam name="TEntity">The type of the entity being queried.</typeparam>
/// <typeparam name="TKey">The type of the key used for ordering.</typeparam>
/// <param name="Key">An expression that specifies the ordering key.</param>
/// <param name="IsPrimary">Indicates whether this is the primary ordering key.</param>
/// <param name="Mode">The mode of ordering (ascending or descending).</param>
public record ResultOrdering<TEntity, TKey>(
    Expression<Func<TEntity, TKey>> Key,
    bool IsPrimary,
    OrderMode Mode = OrderMode.Ascending) where TEntity : class;

/// <summary>
/// Provides a query builder with support for filtering, ordering, including navigation properties, and projections.
/// </summary>
/// <typeparam name="TEntity">The type of the entity being queried.</typeparam>
public class Query<TEntity> where TEntity : class
{
    private IQueryable<TEntity> _query;
    private readonly List<ResultOrdering<TEntity, object>> _resultOrdering;
    private bool _returnDistinct;
    private bool _noTracking = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="Query{TEntity}"/> class using a data source context.
    /// </summary>
    /// <param name="context">The data source context.</param>
    private Query(EFDataSourceContext context)
    {
        _query = context.Entities<TEntity>();
        _resultOrdering = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Query{TEntity}"/> class using an existing <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <param name="query">The existing queryable source.</param>
    private Query(IQueryable<TEntity> query)
    {
        _query = query;
        _resultOrdering = [];
    }

    /// <summary>
    /// Creates a new <see cref="Query{TEntity}"/> from an existing <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <param name="query">The existing queryable source.</param>
    /// <returns>A new query instance.</returns>
    public static Query<TEntity> FromQueryable(IQueryable<TEntity> query)
    {
        return new Query<TEntity>(query);
    }

    /// <summary>
    /// Creates a new <see cref="Query{TEntity}"/> from a <see cref="EFDataSourceContext"/>.
    /// </summary>
    /// <param name="context">The data source context.</param>
    /// <returns>A new query instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is null.</exception>
    public static Query<TEntity> FromContext(EFDataSourceContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new Query<TEntity>(context);
    }

    /// <summary>
    /// Adds a filtering condition to the query.
    /// </summary>
    /// <param name="predicate">The filter predicate.</param>
    /// <returns>The updated query instance.</returns>
    public Query<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        _query = _query.Where(predicate);
        return this;
    }

    /// <summary>
    /// Adds an ordering to the query.
    /// </summary>
    /// <param name="keySelector">The key selector for ordering.</param>
    /// <param name="mode">The ordering mode (ascending or descending).</param>
    /// <returns>The updated query instance.</returns>
    public Query<TEntity> OrderBy(Expression<Func<TEntity, object>> keySelector, OrderMode mode = OrderMode.Ascending)
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        _resultOrdering.Add(
            new ResultOrdering<TEntity, object>(
                keySelector,
                _resultOrdering.Count == 0,
                mode
            )
        );
        return this;
    }

    /// <summary>
    /// Adds a secondary ordering to the query.
    /// </summary>
    /// <param name="keySelector">The key selector for ordering.</param>
    /// <param name="mode">The ordering mode (ascending or descending).</param>
    /// <returns>The updated query instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no primary ordering is defined.</exception>
    public Query<TEntity> ThenBy(Expression<Func<TEntity, object>> keySelector, OrderMode mode = OrderMode.Ascending)
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        if (_resultOrdering.Count == 0)
        {
            throw new InvalidOperationException("Method can only be called after OrderBy!");
        }
        _resultOrdering.Add(new ResultOrdering<TEntity, object>(keySelector, false, mode));
        return this;
    }

    /// <summary>
    /// Includes a navigation property in the query.
    /// </summary>
    /// <param name="navigationPropertyPath">The navigation property path.</param>
    /// <returns>The updated query instance.</returns>
    public Query<TEntity> Include(Expression<Func<TEntity, object>> navigationPropertyPath)
    {
        ArgumentNullException.ThrowIfNull(navigationPropertyPath);
        _query = _query.Include(navigationPropertyPath);
        return this;
    }


    /// <summary>
    /// Adds a ThenInclude clause to the query for a specified sub-entity navigation property.
    /// </summary>
    /// <typeparam name="TPreviousProperty">The type of the previous navigation property.</typeparam>
    /// <typeparam name="TNextProperty">The type of the next navigation property.</typeparam>
    /// <param name="subEntityPath">An expression specifying the sub-entity navigation property to include.</param>
    /// <returns>The current <see cref="Query{TEntity}"/> instance with the ThenInclude clause applied.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="subEntityPath"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the current query is not in an includable state. Ensure the main property is included first.
    /// </exception>
    public Query<TEntity> ThenIncludeSubEntity<TPreviousProperty, TNextProperty>(
        Expression<Func<TPreviousProperty, TNextProperty>> subEntityPath)
    {
        ArgumentNullException.ThrowIfNull(subEntityPath);

        if (_query is IIncludableQueryable<TEntity, TPreviousProperty> includable)
        {
            _query = includable.ThenInclude(subEntityPath);
        }
        else
        {
            throw new InvalidOperationException("Query is not in includable state. Include main property first");
        }

        return this;
    }


    /// <summary>
    /// Marks the query to return DISTINCT results i.e. no duplicate results.
    /// </summary>
    /// <returns>Current instance with DISTINCT modifier applied</returns>
    public Query<TEntity> RemoveDuplicateResults()
    {
        _returnDistinct = true;
        return this;
    }


    /// <summary>
    /// Marks the query to disable EF tracking.
    /// CAUTION: MUST MANUALLY ATTACH ENTITIES FROM THE QUERY TO BE ABLE TO USE THEM WITHIN EF.
    /// </summary>
    /// <returns>Current instance with Entity Tracking disabled.</returns>
    public Query<TEntity> AsNoTracking()
    {
        if (!_noTracking)
        {
            _noTracking = true;
        }
        return this;
    }


    /// <summary>
    /// Projects the query results to a new type.
    /// </summary>
    /// <typeparam name="TNew">The type to project to.</typeparam>
    /// <param name="selector">The projection selector.</param>
    /// <returns>An <see cref="IQueryable{T}"/> of the projected results.</returns>
    public IQueryable<TNew> ProjectTo<TNew>(Expression<Func<TEntity, TNew>> selector) where TNew : class
    {
        return BuildOrderedQuery(_query).Select(selector);
    }

    /// <summary>
    /// Returns the query as an <see cref="IQueryable{T}"/> without executing it.
    /// </summary>
    /// <returns>The <see cref="IQueryable{TEntity}"/> represented by the Query.</returns>
    public IQueryable<TEntity> ToQueryable()
    {
        return BuildOrderedQuery(_query);
    }

    


    /// <summary>
    /// Applies all ordering operations to the query.
    /// </summary>
    /// <param name="query">The query to compile.</param>
    /// <returns>The compiled query with applied orderings.</returns>
    private IQueryable<TEntity> BuildOrderedQuery(IQueryable<TEntity> query)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (_resultOrdering.Count <= 0) return query;

        var theQuery = query;

        foreach (var resultOrdering in _resultOrdering)
        {
            if (resultOrdering.IsPrimary)
            {
                theQuery = resultOrdering.Mode == OrderMode.Ascending
                    ? theQuery.OrderBy(resultOrdering.Key)
                    : theQuery.OrderByDescending(resultOrdering.Key);
            }
            else
            {
                var orderedQuery = (theQuery as IOrderedQueryable<TEntity>)!;
                theQuery = resultOrdering.Mode == OrderMode.Ascending
                    ? orderedQuery.ThenBy(resultOrdering.Key)
                    : orderedQuery.ThenByDescending(resultOrdering.Key);
            }
        }
        return _returnDistinct ? 
            theQuery.Distinct() : 
            theQuery;
    }
}

/// <summary>
/// Specifies the mode of ordering.
/// </summary>
public enum OrderMode : ushort
{
    /// <summary>
    /// Ascending order.
    /// </summary>
    Ascending = 0,

    /// <summary>
    /// Descending order.
    /// </summary>
    Descending = 1
}

public static class QueryLinqExtensions
{   
    
    /// <summary>
    /// Runs the query against the underlying data source and returns the results as a list.
    /// </summary>
    /// <returns>List of elements matched by the query.</returns>
    public static List<TEntity> ToList<TEntity>(this Query<TEntity> query) where TEntity:class
    {
        ArgumentNullException.ThrowIfNull(query);
        return query.ToQueryable().ToList();
    }


    /// <summary>
    /// Determines whether any results matched by the query satisfy the given predicate asynchronously.
    /// </summary>
    /// <param name="predicate">The predicate to test against.</param>
    /// <returns>True or False based on whether any elements passed the predicate.</returns>
    public static bool Any<TEntity>(this Query<TEntity> query, Expression<Func<TEntity, bool>> predicate) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(predicate);
        return query.ToQueryable()
            .Any(predicate);
    }

    /// <summary>
    /// Determines whether all results matched by the query satisfy the given predicate.
    /// </summary>
    /// <param name="predicate">The predicate to test against.</param>
    /// <returns>True if all elements match, false otherwise.</returns>
    public static bool All<TEntity>(this Query<TEntity> query, Expression<Func<TEntity, bool>> predicate) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(predicate);

        return query.ToQueryable()
            .All(predicate);
    }

    /// <summary>
    /// Returns the first element of the query.
    /// </summary>
    /// <typeparam name="TEntity">The type of entities contained by the Query.</typeparam>
    /// <param name="query">The query from which to get the first element</param>
    /// <returns>The first element in the query results.</returns>
    /// <exception cref="ArgumentNullException">If the query is null.</exception>
    /// <exception cref="InvalidOperationException">If the query returned no results.</exception>
    public static TEntity? First<TEntity>(this Query<TEntity> query) where TEntity: class
    {
        ArgumentNullException.ThrowIfNull(query);
        return query.ToQueryable()
            .First();
    }

    /// <summary>
    /// Retrieves the first element of the query.
    /// </summary>
    /// <typeparam name="TEntity">The type of entities fetched by the Query.</typeparam>
    /// <param name="query">The query from which to get the first element.</param>
    /// <returns>The first element in the query results - if no resultswere returned will return <see cref="default"/>.</returns>
    /// <exception cref="ArgumentNullException">If called on a <see cref="null"/> Query/ </exception>
    public static TEntity? FirstOrDefault<TEntity>(this Query<TEntity> query) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(query);
        return query.ToQueryable()
            .FirstOrDefault();
    }

    /// <summary>
    /// Retrieves the first and only element of the query.
    /// </summary>
    /// <typeparam name="TEntity">Entity type of elements in the query</typeparam>
    /// <param name="query">The query whose execution will return results.</param>
    /// <returns>The only element returned by the query.</returns>
    /// <exception cref="InvalidOperationException">If the query matched no records or returned multiple records.</exception>"
    public static TEntity? Single<TEntity>(this Query<TEntity> query) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(query);
        return query.ToQueryable()
            .Single();
    }

    /// <summary>
    /// Retrieves the first and only element of the query.
    /// 
    /// </summary>
    /// <typeparam name="TEntity">The type of entities contained by the underlying store.</typeparam>
    /// <param name="query"></param>
    /// <returns>First and only element of the query - returns <see cref="default"/> if no results were returned. </returns>
    /// <exception cref="InvalidOperationException">If the query matched multiple records.</exception>
    public static TEntity? SingleOrDefault<TEntity>(this Query<TEntity> query) where TEntity:class
    {
        ArgumentNullException.ThrowIfNull(query);
        return query.ToQueryable()
            .SingleOrDefault();
    }


    /// <summary>
    /// Counts the number of elements in the query.
    /// </summary>
    /// <typeparam name="TEntity">The type of entities the query expects.</typeparam>
    /// <param name="query">The query on which to call the method.</param>
    /// <returns>The number of records matched by the query.</returns>
    public static int Count<TEntity>(this Query<TEntity> query) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(query);
        return query.ToQueryable()
            .Count();
    }

    /// <summary>
    /// Counts the number of elements in the query.
    /// For cases where the expected count is larger than <see cref="int.MaxValue"/>
    /// </summary>
    /// <typeparam name="TEntity">The type of entities the query expects to work on.</typeparam>
    /// <param name="query">The query on which the method is called.</param>
    /// <returns>The number of records matched by the query.</returns>
    public static long LongCount<TEntity>(this Query<TEntity> query) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(query);
        return query.ToQueryable()
            .LongCount();
    }


    /// <summary>
    /// Runs the query against the underlying data source asynchronously and returns the results as a list.
    /// </summary>
    /// <returns>Task containing List of elements matched by the query.</returns>
    public static Task<List<TEntity>> ToListAsync<TEntity>(this Query<TEntity> query) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(query);
        return query.ToQueryable().ToListAsync();
    }



    /// <summary>
    /// Retrieves the first element of the query.
    ///
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>Task containing the first element of the query.</returns>
    /// <exception cref="InvalidOperationException">If there are no results in the query.</exception>
    public static async Task<TEntity?> FirstAsync<TEntity>(this Query<TEntity> query, CancellationToken cancellationToken = default) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(query);
        return await query.ToQueryable()
                    .FirstAsync(cancellationToken);
    }

    /// <summary>
    /// Retries the ONLY element of the query.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>Task containing the only element found by the query.</returns>
    /// <exception cref="InvalidOperationException">If there query matched no records or returned multiple records.</exception>
    public static async Task<TEntity?> SingleAsync<TEntity>(this Query<TEntity> query, CancellationToken cancellationToken = default) where TEntity:class
    {
        ArgumentNullException.ThrowIfNull(query);
        return await query.ToQueryable()
                    .SingleAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves the first element of the query.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>The first element of the query(if any) or default if no elements found.</returns>
    public static async Task<TEntity?> FirstOrDefaultAsync<TEntity>(this Query<TEntity> query, CancellationToken cancellationToken = default) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(query);
        return await query.ToQueryable()
                    .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves the first element of the query.
    /// Will return default if no elements are found.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>Task containing the matched element, default if no elements were found.</returns>
    /// <exception cref="InvalidOperationException">Thrown if multiple elements are found.</exception>
    public static async Task<TEntity?> SingleOrDefaultAsync<TEntity>(this Query<TEntity> query, CancellationToken cancellationToken = default) where TEntity:class
    {
        ArgumentNullException.ThrowIfNull(query);
        return await query.ToQueryable()
            .SingleOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Counts the number of elements in the query asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the count.</param>
    /// <returns>Task containing number of elements in query.</returns>
    public static async Task<int> CountAsync<TEntity>(this Query<TEntity> query, CancellationToken cancellationToken = default) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(query);
        return await query.ToQueryable()
            .CountAsync(cancellationToken);
    }

    /// <summary>
    /// Counts the number of elements in the query asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the count</param>
    /// <returns>Task containing number of elements in query.</returns>
    public static async Task<long> LongCountAsync<TEntity>(this Query<TEntity> query, CancellationToken cancellationToken = default) where TEntity: class
    {
        ArgumentNullException.ThrowIfNull(query);
        return await query.ToQueryable()
            .LongCountAsync(cancellationToken);
    }

    /// <summary>
    /// Determines whether any results matched by the query asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Token for cancelling the async operation.</param>
    /// <returns>Task containing the boolean result.</returns>
    public static async Task<bool> AnyAsync<TEntity>(this Query<TEntity> query, CancellationToken cancellationToken = default) where TEntity:class
    {
        ArgumentNullException.ThrowIfNull(query);
        return await query
            .ToQueryable()
            .AnyAsync(cancellationToken);
    }


    /// <summary>
    /// Determines whether any results matched by the query satisfy the given predicate asynchronously.
    /// </summary>
    /// <param name="predicate">The predicate to test results against.</param>
    /// <param name="cancellationToken">Token for cancelling the async operation.</param>
    /// <returns>Task containing the boolean result.</returns>
    public static async Task<bool> AnyAsync<TEntity>(this Query<TEntity> query, 
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default) where TEntity: class
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(predicate);
        return await query.ToQueryable().AnyAsync(predicate, cancellationToken);
    }



    /// <summary>
    /// Determines whether all results matched by the query satisfy the given predicate asynchronously.
    /// </summary>
    /// <param name="predicate">The predicate to test against.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>Task containing the result of the async operation.</returns>
    public static async Task<bool> AllAsync<TEntity>(this Query<TEntity> query, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) where TEntity:class
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(predicate);
        return await query.ToQueryable()
            .AllAsync(predicate, cancellationToken);
    }



}
