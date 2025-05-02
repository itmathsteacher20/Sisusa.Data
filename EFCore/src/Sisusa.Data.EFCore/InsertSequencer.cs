using Microsoft.EntityFrameworkCore;


namespace Sisusa.Data.EFCore
{
    /// <summary>
    /// A builder class for inserting entities into a database context.
    /// </summary>
    public class InsertBuilder
    {
        private readonly List<object> _itemsToAdd;

        private InsertBuilder()
        {
            _itemsToAdd = new();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="InsertBuilder"/> class.
        /// </summary>
        /// <returns>
        /// The builder instance.
        /// </returns>
        public static InsertBuilder Create()
        {
            return new InsertBuilder();
        }

        /// <summary>
        /// Inserts the specified entity into the database context.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity to add.</typeparam>
        /// <param name="item">The entity to insert/add</param>
        /// <returns>The current builder instance for chaining further inserts.</returns>
        /// <exception cref="ArgumentNullException">If the item is null.</exception>
        public static InsertBuilder FirstInsert<TEntity>(TEntity item)
        {
            return InsertBuilder.Create()
                                .Insert(item);
        }

        /// <summary>
        /// Inserts the specified entity into the database context.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity to add to the database.</typeparam>
        /// <param name="item">The entity to add/insert to the database.</param>
        /// <returns>Current builder instance for further inserts.</returns>
        /// <exception cref="ArgumentNullException">If the item is null.</exception>
        public InsertBuilder Insert<TEntity>(TEntity item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            _itemsToAdd.Add(item);
            return this;
        }

        /// <summary>
        /// Inserts the specified entity into the database context.
        /// This is used after an initial insert.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity to add.</typeparam>
        /// <param name="item">The entity being added.</param>
        /// <returns>Current instance for further inserts.</returns>
        /// <exception cref="ArgumentNullException">If given item is null.</exception>
        /// <exception cref="InvalidOperationException">If called before Insert.</exception>
        public InsertBuilder ThenInsert<TEntity>(TEntity item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (_itemsToAdd.Count == 0)
                throw new InvalidOperationException("You must call Insert() before ThenInsert().");

            _itemsToAdd.Add(item);
            return this;
        }

        /// <summary>
        /// Inserts all entities in the specified collection into the database context.
        /// </summary>
        /// <typeparam name="TEntity">The data type of entities being added.</typeparam>
        /// <param name="entities">The entities being added to the database.</param>
        /// <returns>The current builder instance.</returns>
        public InsertBuilder InsertAll<TEntity>(IEnumerable<TEntity> entities)
        {
            ArgumentNullException.ThrowIfNull(entities);

            foreach (var entity in entities)
            {
                Insert(entity);
            }
            return this;
        }

        /// <summary>
        /// Executes the insert operation on the specified database context.
        /// </summary>
        /// <param name="context">The database context to which the items are being added.</param>
        /// <param name="cancellationToken">Token to observe while performing the operation.</param>
        /// <returns>Task representing state of the operation.</returns>
        /// <exception cref="InvalidOperationException">If nothing has been added yet.</exception>
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




    }
}
