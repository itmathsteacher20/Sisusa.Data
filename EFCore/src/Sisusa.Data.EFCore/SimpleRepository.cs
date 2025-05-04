using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Sisusa.Data.Contracts;
using Sisusa.Data.EFCore.Exceptions;

namespace Sisusa.Data.EFCore;

/// <inheritdoc cref="IRepository{T, TId}"/>
/// <summary>
/// A strongly typed repository implementation
/// </summary>
/// <typeparam name="TEntity">The type of entities contained in the repository</typeparam>
/// <typeparam name="TId">The key type</typeparam>
public class SimpleRepository<TEntity, TId> : IRepository<TEntity, TId> where TEntity : class
{
    private readonly EFDataSourceContext _context;
    
    private readonly EntitySet<TEntity> _dbSet;

    /// <summary>
    /// The underlying dbcontext
    /// </summary>
    public EFDataSourceContext Context => _context;

    public SimpleRepository(EFDataSourceContext dbContext)
    {
        _context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        
        _dbSet = new EntitySet<TEntity>(_context.Set<TEntity>(), _context);
    }

    public virtual async Task<TEntity?> FindByIdAsync(TId id)
    {
        ArgumentNullException.ThrowIfNull(id, nameof(id));
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<bool> HasByIdAsync(TId id)
    {
        ArgumentNullException.ThrowIfNull(id, nameof(id));
        var entity = await _dbSet.FindAsync(id);
        return entity != null;
    }

    public virtual async Task<ICollection<TEntity>> FindAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<ICollection<TEntity>> FindAllByFilter(Expression<Func<TEntity, bool>> filter)
    {
        return await _dbSet
            .Where(filter)
            .ToListAsync();
    }

    public virtual async Task<int> CountAsync()
    {
        return await _dbSet
            .CountAsync();
    }

    public virtual async Task<int> CountByFilterAsync(Expression<Func<TEntity, bool>> filter)
    {
        return await _dbSet
            .Where(filter)
            .CountAsync();
    }

    public virtual async Task UpdateAsync(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        
        _context.Entry(entity).State = EntityState.Modified;
        
        await _context.SaveChangesAsync();
    }

    public async Task UpdateByIdAsync(TId id, TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(id, nameof(id));
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        
        var existing = await FindByIdAsync(id);
        if (existing == null)
            throw new EntityNotFoundException();
        
        var properties = entity.GetType().GetProperties();
        var changed = properties
            .Any(pInfo =>
            {
                var propertyValue = pInfo.GetValue(existing, null);
                var updatedValue = pInfo.GetValue(entity, null);
                if (propertyValue == null)
                {
                    return updatedValue != null;
                }
                return propertyValue!.Equals(updatedValue);
            });
        
        if (!changed)
            return;
        
        _context.Entry(existing).CurrentValues.SetValues(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteByIdAsync(TId id)
    {
        ArgumentNullException.ThrowIfNull(id, nameof(id));
        
        var entity = await _dbSet.FindAsync(id);
        if (entity == null)
            throw new EntityNotFoundException();
        
        await _dbSet.RemoveAsync(entity);
    }

    public virtual async Task AddNewAsync(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        await _dbSet.AddAsync(entity);
    }
}