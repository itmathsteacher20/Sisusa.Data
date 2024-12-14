using System.Collections;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Sisusa.Data.Contracts;

namespace Sisusa.Data.EFCore;

/// <summary>
///  An implementation of <see cref="IEntityCollection{TEntity}"/> for use with an EF context
/// </summary>
/// <typeparam name="T">The type of entities in the collection/set</typeparam>
/// <param name="wrappedSet">The underlying <see cref="DbSet{TEntity}"/> to  use</param>
/// <param name="sourceContext">The <see cref="IDataSourceContext"/> that handles the database operations.</param>
/// <inheritdoc />
public class EntitySet<T>(DbSet<T> wrappedSet, IDataSourceContext sourceContext) : IQueryable<T>, IEntityCollection<T> where T: class 
{
    
    public async Task<T> SingleAsync()
    {
        return await  wrappedSet.SingleAsync();
    }

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        return await wrappedSet.AnyAsync(predicate);
    }
    
    public async Task<bool> AnyAsync() => await wrappedSet.AnyAsync();

    public IQueryable<T> Where(Expression<Func<T, bool>> predicate)
    {
        return wrappedSet.Where(predicate);
    }

    public async Task<T> FirstAsync(Expression<Func<T, bool>> predicate)
    {
        return await wrappedSet.FirstAsync(predicate);
    }
    
    public async Task<T> FirstAsync() => await wrappedSet.FirstAsync();

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await wrappedSet.FirstOrDefaultAsync(predicate);
    }
    
    public async Task<T?> FirstOrDefaultAsync() => await wrappedSet.FirstOrDefaultAsync();

    public IEnumerable<T> AsEnumerable()
    {
        return wrappedSet.AsEnumerable();
    }

    public async Task<List<T>> ToListAsync()
    {
        return await wrappedSet.ToListAsync();
    }

    public async Task<int> CountAsync()
    {
        return await wrappedSet.CountAsync();
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await wrappedSet.AddAsync(entity, cancellationToken);
        await sourceContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await wrappedSet.AddRangeAsync(entities, cancellationToken);
        await sourceContext.SaveChangesAsync(cancellationToken);
    }

    public void Remove(T entity)
    {
        wrappedSet.Entry(entity).State = EntityState.Deleted;
        sourceContext.SaveChanges();
    }

    public async Task RemoveAsync(T entity, CancellationToken cancellationToken = default)
    {
        this.Entry(entity).State = EntityState.Deleted;
        await sourceContext.SaveChangesAsync(cancellationToken);
    }

    internal DbSet<T> GetWrappedSet() => wrappedSet;


    public int Count()
    {
        return wrappedSet.Count();
    }

    public long LongCount()
    {
        return wrappedSet.LongCount();
    }

    public async Task<long> LongCountAsync()
    {
        return await wrappedSet.LongCountAsync();
    }

    public void RemoveRange(IEnumerable<T> entities)
    {
        wrappedSet.RemoveRange(entities); 
        sourceContext.SaveChanges();
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        //wrappedSet.Entry(entity).State = EntityState.Modified;
        wrappedSet.Update(entity);
        await sourceContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<T?> FindAsync(params object[] keyValues)
    {
        return await wrappedSet.FindAsync(keyValues);
    }

    public static implicit operator DbSet<T>(EntitySet<T> entitySet)
    {
        return entitySet.GetWrappedSet();
    }

#region IQueryable
    public IEnumerator<T> GetEnumerator()
    {
        return wrappedSet
            .AsEnumerable()
            .GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public Type ElementType => typeof(T);

    public Expression Expression => ((IQueryable<T>)wrappedSet).Expression; //{ get; }

    public IQueryProvider Provider => ((IQueryable<T>)wrappedSet).Provider;
    
    #endregion
}

/// <summary>
/// Provides extension methods for working with <see cref="EntitySet{T}"/> that simplify querying and entity management.
/// </summary>
public static class EntitySetExtensions
{
    /// <summary>
    /// Attaches the specified entity to the context in the <see cref="EntitySet{T}"/>.
    /// The entity's state is set to <see cref="EntityState.Unchanged"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to attach.</typeparam>
    /// <param name="eSet">The <see cref="EntitySet{T}"/> to attach the entity to.</param>
    /// <param name="entity">The entity to attach.</param>
    /// <returns>The <see cref="EntityEntry{TEntity}"/> for the entity.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="entity"/> is <c>null</c>.</exception>
    public static EntityEntry<TEntity> Attach<TEntity>(this EntitySet<TEntity> eSet, TEntity entity) where TEntity : class
    {
        var dbSet = eSet.GetWrappedSet();
        return dbSet.Attach(entity);
    }

    /// <summary>
    /// Returns the maximum value of a given property for entities in the <see cref="EntitySet{T}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity.</typeparam>
    /// <typeparam name="TMax">The type of the property to calculate the maximum value for.</typeparam>
    /// <param name="eSource">The source <see cref="EntitySet{T}"/> to perform the operation on.</param>
    /// <param name="selector">The expression to select the property to calculate the maximum value for.</param>
    /// <returns>The maximum value of the specified property.</returns>
    public static TMax? Max<TEntity, TMax>(this EntitySet<TEntity> eSource, Expression<Func<TEntity, TMax>> selector) where TEntity : class
    {
        return eSource.GetWrappedSet().Max(selector);
    }

    /// <summary>
    /// Returns the minimum value of a given property for entities in the <see cref="EntitySet{T}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity.</typeparam>
    /// <typeparam name="TMin">The type of the property to calculate the minimum value for.</typeparam>
    /// <param name="eSource">The source <see cref="EntitySet{T}"/> to perform the operation on.</param>
    /// <param name="selector">The expression to select the property to calculate the minimum value for.</param>
    /// <returns>The minimum value of the specified property.</returns>
    public static TMin? Min<TEntity, TMin>(this EntitySet<TEntity> eSource, Expression<Func<TEntity, TMin>> selector) where TEntity : class
    {
        return eSource.GetWrappedSet().Min(selector);
    }

    /// <summary>
    /// Asynchronously returns the maximum value of a given property for entities in the <see cref="EntitySet{T}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity.</typeparam>
    /// <typeparam name="TMax">The type of the property to calculate the maximum value for.</typeparam>
    /// <param name="eSource">The source <see cref="EntitySet{T}"/> to perform the operation on.</param>
    /// <param name="selector">The expression to select the property to calculate the maximum value for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the maximum value of the specified property.</returns>
    public static async Task<TMax?> MaxAsync<TEntity, TMax>(this EntitySet<TEntity> eSource, Expression<Func<TEntity, TMax>> selector) where TEntity : class
    {
        return await eSource.GetWrappedSet().MaxAsync(selector);
    }

    /// <summary>
    /// Asynchronously returns the minimum value of a given property for entities in the <see cref="EntitySet{T}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity.</typeparam>
    /// <typeparam name="TMin">The type of the property to calculate the minimum value for.</typeparam>
    /// <param name="eSource">The source <see cref="EntitySet{T}"/> to perform the operation on.</param>
    /// <param name="selector">The expression to select the property to calculate the minimum value for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the minimum value of the specified property.</returns>
    public static async Task<TMin?> MinAsync<TEntity, TMin>(this EntitySet<TEntity> eSource, Expression<Func<TEntity, TMin>> selector) where TEntity : class
    {
        return await eSource.GetWrappedSet().MinAsync(selector);
    }

    /// <summary>
    /// Asynchronously calculates the sum of a given decimal property for entities in the <see cref="EntitySet{T}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity.</typeparam>
    /// <param name="source">The source <see cref="EntitySet{T}"/> to perform the operation on.</param>
    /// <param name="selector">The expression to select the property to sum.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the sum of the selected property.</returns>
    public static async Task<decimal> SumAsync<TEntity>(this EntitySet<TEntity> source, Expression<Func<TEntity, decimal>> selector) where TEntity : class
    {
        return await source.GetWrappedSet().SumAsync(selector);
    }

    /// <summary>
    /// Calculates the sum of a given decimal property for entities in the <see cref="EntitySet{T}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity.</typeparam>
    /// <param name="source">The source <see cref="EntitySet{T}"/> to perform the operation on.</param>
    /// <param name="selector">The expression to select the property to sum.</param>
    /// <returns>The sum of the selected property.</returns>
    public static decimal Sum<TEntity>(this EntitySet<TEntity> source, Expression<Func<TEntity, decimal>> selector) where TEntity : class
    {
        return source.GetWrappedSet().Sum(selector);
    }

    /// <summary>
    /// Calculates the average value of a given double property for entities in the <see cref="EntitySet{T}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity.</typeparam>
    /// <param name="source">The source <see cref="EntitySet{T}"/> to perform the operation on.</param>
    /// <param name="selector">The expression to select the property to calculate the average value for.</param>
    /// <returns>The average value of the selected property.</returns>
    public static double Average<TEntity>(this EntitySet<TEntity> source, Expression<Func<TEntity, double>> selector) where TEntity : class
    {
        return source.GetWrappedSet().Average(selector);
    }

    /// <summary>
    /// Asynchronously calculates the average value of a given double property for entities in the <see cref="EntitySet{T}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity.</typeparam>
    /// <param name="source">The source <see cref="EntitySet{T}"/> to perform the operation on.</param>
    /// <param name="selector">The expression to select the property to calculate the average value for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the average value of the selected property.</returns>
    public static async Task<double> AverageAsync<TEntity>(this EntitySet<TEntity> source, Expression<Func<TEntity, double>> selector) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));
        return await source.GetWrappedSet().AverageAsync(selector);
    }

    /// <summary>
    /// Retrieves the <see cref="EntityEntry{TEntity}"/> for a specified entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="source">The <see cref="EntitySet{T}"/> to perform the operation on.</param>
    /// <param name="entity">The entity whose <see cref="EntityEntry{TEntity}"/> is to be retrieved.</param>
    /// <returns>The <see cref="EntityEntry{TEntity}"/> for the specified entity.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="entity"/> is <c>null</c>.</exception>
    public static EntityEntry<TEntity> Entry<TEntity>(this EntitySet<TEntity> source, TEntity entity) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        return source.GetWrappedSet().Entry(entity);
    }
}

