using Microsoft.EntityFrameworkCore;
using Sisusa.Data.Contracts;
using System.Linq.Expressions;
using Sisusa.Data.EFCore.Exceptions;
using System.Reflection;

namespace Sisusa.Data.EFCore
{
    public class BaseRepository<TEntity, TId> : IRepository<TEntity, TId> where TEntity : class
    {    
        private readonly DbSet<TEntity> _entities;
             
        private readonly EFDataSourceContext _context;

        private readonly Func<TId, TEntity> _createProxy;

        /// <summary>
        /// The underlying dbcontext
        /// </summary>
        public EFDataSourceContext Context => _context;

        public BaseRepository(
            EFDataSourceContext dataSourceContext,
            Func<TId, TEntity> createProxy
            )
        {
            _context = dataSourceContext ?? 
                       throw new ArgumentNullException(nameof(dataSourceContext));
            _entities = _context.Set<TEntity>();
            
            _createProxy = createProxy ?? throw new ArgumentNullException(nameof(createProxy));
        }


        private PropertyInfo GetIdProperty()
        {
            bool IsPossibleId(PropertyInfo p)
            {
                return p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) ||
                p.Name.Equals($"{typeof(TEntity).Name}Id", StringComparison.OrdinalIgnoreCase);
            }

            var idProperty = typeof(TEntity).GetProperties()
                .FirstOrDefault(p => IsPossibleId(p));

            return idProperty == null ? 
                throw new InvalidOperationException($"No Id property found for {typeof(TEntity).Name}") :
                idProperty;
        }

        public async Task AddNewAsync(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            
            await _entities.AddAsync(entity);
        }

        public async Task<int> CountAsync()
        {
            return await _entities
                .CountAsync();
        }

        public async Task<int> CountByFilterAsync(Expression<Func<TEntity, bool>> filter)
        {
            ArgumentNullException.ThrowIfNull(filter, nameof(filter));
            return await _entities
                .Where(filter)
                .CountAsync();
        }

        public async Task UpdateByIdAsync(TId id, TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            ArgumentNullException.ThrowIfNull(id, nameof(id));
            
            var existing = await FindByIdAsync(id);
            if (existing == null)
                throw new EntityNotFoundException($"Entity with id {id} not found. Update cancelled.");
            
            var properties = typeof(TEntity).GetProperties();
            var changed = false;
            foreach (var property in properties)
            {
                var oldValue = property.GetValue(existing, null);
                var newValue = property.GetValue(entity, null);
                
                changed = oldValue != newValue;
                if (changed) break;
            }

            if (!changed)
                return;
            
            _context.Entry(existing).CurrentValues.SetValues(entity);
            _context.Entry(existing).State = EntityState.Modified;
            
            await _context.SaveChangesAsync();
        }

        public async Task DeleteByIdAsync(TId id)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));
            var existing = await FindByIdAsync(id) ?? throw new EntityNotFoundException();
            _context.Entry(existing!).State = EntityState.Deleted;
            await _context.SaveChangesAsync();
        }

        public async Task<ICollection<TEntity>> FindAllAsync()
        {
            return await _entities
                .ToListAsync();
        }

        public async Task<ICollection<TEntity>> FindAllByFilter(Expression<Func<TEntity, bool>> filter)
        {
            ArgumentNullException.ThrowIfNull(filter, nameof(filter));
            return await _entities
                .Where(filter)
                .ToListAsync();
        }

        public async Task<TEntity?> FindByIdAsync(TId id)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));
            
            return await _entities
                .FindAsync(id);
        }

        public async Task<bool> HasByIdAsync(TId id)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));
            var item = await _entities.FindAsync(id);
            return item != null;
        }

        public async Task UpdateAsync(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            _entities.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
