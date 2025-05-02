using Microsoft.EntityFrameworkCore;


namespace Sisusa.Data.EFCore
{
    public class InsertBuilder
    {
        private readonly List<object> _itemsToAdd;

        private InsertBuilder()
        {
            _itemsToAdd = new();
        }

        public static InsertBuilder Create()
        {
            return new InsertBuilder();
        }

        public InsertBuilder Insert<TEntity>(TEntity item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            _itemsToAdd.Add(item);
            return this;
        }

        public InsertBuilder ThenInsert<TEntity>(TEntity item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (_itemsToAdd.Count == 0)
                throw new InvalidOperationException("You must call Insert() before ThenInsert().");

            _itemsToAdd.Add(item);
            return this;
        }

        public async Task ExecuteAsync(DbContext context, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (_itemsToAdd.Count == 0)
                throw new InvalidOperationException("No items to insert.");
            int bulkMinSize = 500;

            if (_itemsToAdd.Count < bulkMinSize)
            {
                context.AddRange(_itemsToAdd);
                
            }
            await context.SaveChangesAsync(cancellationToken);
            _itemsToAdd.Clear();
            return;
        }

        public InsertBuilder InsertAll<TEntity>(IEnumerable<TEntity> entities)
        {
            ArgumentNullException.ThrowIfNull(entities);

            foreach (var entity in entities)
            {
                Insert(entity);
            }
            return this;
        }


    }
}
