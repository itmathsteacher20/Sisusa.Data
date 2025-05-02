using System.Collections;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Sisusa.Data.Contracts;

namespace Sisusa.Data.EFCore;

/// <summary>
///  An implementation of <see cref="IEntityCollection{TEntity}"/> for use with an EF context
/// </summary>
/// <typeparam name="T">The type of entities in the collection/set</typeparam>
/// <param name="wrappedSet">The underlying <see cref="DbSet{TEntity}"/> to  use</param>
/// <param name="sourceContext">The <see cref="IDataSourceContext"/> that handles the database operations.</param>
/// <inheritdoc />
public class EntitySet<T>(DbSet<T> wrappedSet, DbContext sourceContext) : IQueryable<T>, IEntityCollection<T> where T: class 
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
        using (var transaction = await sourceContext.Database.BeginTransactionAsync(cancellationToken))
        {
            try
            {   
                foreach (var entity in entities)
                {
                    wrappedSet.Add(entity);
                }
                await sourceContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            } 
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

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

    public Task RemoveRangeAsync(IEnumerable<T> entities)
    {
        throw new NotImplementedException();
    }

    public Type ElementType => typeof(T);

    public Expression Expression => ((IQueryable<T>)wrappedSet).Expression; //{ get; }

    public IQueryProvider Provider => ((IQueryable<T>)wrappedSet).Provider;
    
    #endregion
}

/// <summary>
/// Provides extension methods for querying <see cref="EntitySet{TEntity}"/>.
/// </summary>
public static class EntitySetQueryExtensions
{
    /// <summary>
    /// Retrieves all entities from the specified <see cref="EntitySet{TEntity}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities in the set.</typeparam>
    /// <param name="source">The <see cref="EntitySet{TEntity}"/> to query.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing all entities in the set.</returns>
    public static IEnumerable<TEntity> GetAll<TEntity>(this EntitySet<TEntity> source) where TEntity : class
    {
        // Placeholder implementation
        return source;
    }

    /// <summary>
    /// Asynchronously retrieves all entities from the specified <see cref="EntitySet{TEntity}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities in the set.</typeparam>
    /// <param name="source">The <see cref="EntitySet{TEntity}"/> to query.</param>
    /// <returns>A task representing the asynchronous operation, containing all entities in the set.</returns>
    public static async Task<IEnumerable<TEntity>> GetAllAsync<TEntity>(this EntitySet<TEntity> source) where TEntity : class
    {
        return await source.ToListAsync();
    }

    /// <summary>
    /// Asynchronously retrieves a single entity by its identifier(s) from the specified <see cref="EntitySet{TEntity}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity in the set.</typeparam>
    /// <param name="source">The <see cref="EntitySet{TEntity}"/> to query.</param>
    /// <param name="id">The identifier(s) of the entity to retrieve.</param>
    /// <returns>A task representing the asynchronous operation, containing the entity if found, or <see langword="null"/> if not found.</returns>
    public static async Task<TEntity?> GetByIdAsync<TEntity>(this EntitySet<TEntity> source, params object[] id) where TEntity : class
    {
        return await source.FindAsync(id);
    }

    /// <summary>
    /// Asynchronously retrieves entities that match the specified predicate from the <see cref="EntitySet{TEntity}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities in the set.</typeparam>
    /// <param name="source">The <see cref="EntitySet{TEntity}"/> to query.</param>
    /// <param name="predicate">An expression to test each entity for a condition.</param>
    /// <returns>A task representing the asynchronous operation, containing a collection of entities that satisfy the condition.</returns>
    public static async Task<IEnumerable<TEntity>> GetWhereAsync<TEntity>(this EntitySet<TEntity> source, Expression<Func<TEntity, bool>> predicate) where TEntity : class
    {
        return await source.Where(predicate).ToListAsync();
    }
}
