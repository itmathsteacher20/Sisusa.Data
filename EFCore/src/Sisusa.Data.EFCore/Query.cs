using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Sisusa.Data.Contracts;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="Query{TEntity}"/> class using a data source context.
    /// </summary>
    /// <param name="context">The data source context.</param>
    private Query(DataSourceContext context)
    {
        _query = context.Entities<TEntity>();
        _resultOrdering = new List<ResultOrdering<TEntity, object>>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Query{TEntity}"/> class using an existing <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <param name="query">The existing queryable source.</param>
    private Query(IQueryable<TEntity> query)
    {
        _query = query;
        _resultOrdering = new List<ResultOrdering<TEntity, object>>();
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
    /// Creates a new <see cref="Query{TEntity}"/> from a <see cref="DataSourceContext"/>.
    /// </summary>
    /// <param name="context">The data source context.</param>
    /// <returns>A new query instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is null.</exception>
    public static Query<TEntity> FromContext(DataSourceContext context)
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
    /// Projects the query results to a new type.
    /// </summary>
    /// <typeparam name="TNew">The type to project to.</typeparam>
    /// <param name="selector">The projection selector.</param>
    /// <returns>An <see cref="IQueryable{T}"/> of the projected results.</returns>
    public IQueryable<TNew> ProjectTo<TNew>(Expression<Func<TEntity, TNew>> selector) where TNew : class
    {
        return CompileQuery(_query).Select(selector);
    }

    /// <summary>
    /// Projects the query results to a new type without tracking changes.
    /// </summary>
    /// <typeparam name="TNew">The type to project to.</typeparam>
    /// <param name="selector">The projection selector.</param>
    /// <returns>An <see cref="IQueryable{T}"/> of the projected results.</returns>
    public IQueryable<TNew> ProjectAsNoTrackingTo<TNew>(Expression<Func<TEntity, TNew>> selector) where TNew : class
    {
        return CompileQuery(_query).Select(selector).AsNoTracking();
    }

    /// <summary>
    /// Compiles and retrieves the query as an <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <returns>The compiled query.</returns>
    public IQueryable<TEntity> GetCompiled()
    {
        return CompileQuery(_query);
    }

    /// <summary>
    /// Compiles and retrieves the query as an <see cref="IQueryable{T}"/> without tracking changes.
    /// </summary>
    /// <returns>An <see cref="IQueryable{T}"/> of the results.</returns>
    public IQueryable<TEntity> GetCompiledAsNoTracking()
    {
        return CompileQuery(_query).AsNoTracking();
    }

    /// <summary>
    /// Applies all ordering operations to the query.
    /// </summary>
    /// <param name="query">The query to compile.</param>
    /// <returns>The compiled query with applied orderings.</returns>
    private IQueryable<TEntity> CompileQuery(IQueryable<TEntity> query)
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

        return theQuery;
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
