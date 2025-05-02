# Sisusa.Data Unit of Work and Repository Pattern Documentation

## Table of Contents
1. [IDataTransaction](#idatatransaction)
2. [IDataSourceContext](#idatasourcecontext)
3. [IEntityCollection](#ientitycollection)
4. [IRepository](#irepository)
5. [Repository Extensions](#repository-extensions)
   - [IPaginatedRepository](#ipaginatedrepository)
   - [ISearchableRepository](#isearchablerepository)
   - [IRepositoryWithBulkOperations](#irepositorywithbulkoperations)
6. [ITransactionalDataSourceContext](#itransactionaldatasourcecontext)
7. [Commands & Batch Operations](#commands-and-batch-operations)
8. [Use Cases](#use-cases)]

## IDataTransaction

Represents a transaction boundary for data operations. Automatically rolls back if not committed.

**Key Features:**
- Transaction management with commit/rollback
- Both synchronous and asynchronous operations
- Unique transaction ID for logging

### Example Usage (SQLite)

    ```csharp
        // Using SQLite with Dapper
        using (var connection = new SQLiteConnection("Data Source=mydatabase.db"))
        {
            connection.Open();
            var sqliteDataSource = new SqliteDataSourceContext(connection);
    
            // Begin transaction
            using (var unitOfWork = sqliteDataSource.BeginTransaction())
            {
                try
                {
                    var newCustomer = new Customer{Id=202504005, Name="John Doe", Email="john@example.com"};
                    // Perform operations
                    await connection.ExecuteAsync(
                        "INSERT INTO Customers (Id,Name, Email) VALUES (@Id, @Name, @Email)", 
                        newCustomer,
                        transaction);

                    await connection.ExecuteAsync(
                        "INSERT INTO Accounts(CustomerId, Balance) VALUES (@CustomerId, @Balance)")",
                        new { CustomerId = newCustomer.Id, Balance = 10000 },
                        transaction
                    );
                
                    // Commit if successful
                    await unitOfWork.CommitAsync();
                    logger.info($"Transaction {unitOfWork.TransactionId} committed successfully.");
                }
                catch
                {
                    // Automatic rollback occurs if not committed when disposed
                    await unitOfWork.RollbackAsync();
                    logger.error($"Transaction {unitOfWork.TransactionId} rolled back due to an error.");
                    throw;
                }
            }
        }
    ```


## IDataSourceContext

Base interface for data contexts, providing core data access operations.

### Example Usage (CSV File)

```csharp
public class CsvDataSourceContext : IDataSourceContext
{
    private readonly string _filePath;
    
    public CsvDataSourceContext(string filePath)
    {
        _filePath = filePath;
    }
    
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Implementation to write changes to CSV
        return Task.FromResult(1); // Return number of records affected
    }
    
    public IEntityCollection<T> Entities<T>() where T : class
    {
        return new CsvEntityCollection<T>(_filePath);
    }
    
    // ... other members
}

// Usage
var context = new CsvDataSourceContext("customers.csv");
await context.SaveChangesAsync();
```

## IEntityCollection

Represents a queryable collection of entities with CRUD operations.

### Example Usage (SQLite)

```csharp
var context = new SqliteDataSourceContext(connection);
var customers = context.Entities<Customer>();

// Query examples
var activeCustomers = await customers
    .Where(c => c.IsActive)
    .ToListAsync();

var customer = await customers
    .FirstOrDefaultAsync(c => c.Email == "test@example.com");

// Add new customer
await customers.AddAsync(new Customer { Name = "New Customer", Email = "new@example.com" });
await context.SaveChangesAsync();
```

## IRepository

Standard repository pattern with basic CRUD operations.

### Example Implementation (SQL Server)

```csharp
public class CustomerRepository : IRepository<Customer, int>
{
    private readonly AppDbContext _context;
    
    public CustomerRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<Customer?> FindByIdAsync(int id)
    {
        return await _context.Customers.FindAsync(id);
    }
    
    public async Task AddNewAsync(Customer entity)
    {
        await _context.Customers.AddAsync(entity);
        await _context.SaveChangesAsync();
    }
    
    // ... other members
}

// Usage
var repository = new CustomerRepository(dbContext);
var customer = await repository.FindByIdAsync(1);
await repository.UpdateAsync(customer);
```

## Repository Extensions

### IPaginatedRepository

Adds pagination support to repositories.

```csharp
// Implementation for SQL Server
public async Task<ICollection<Customer>> FindAllWithPagingAsync(int page, int pageSize)
{
    return await _context.Customers
        .OrderBy(c => c.Id)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
}

// Usage
var pagedCustomers = await customerRepository.FindAllWithPagingAsync(2, 10); // Page 2, 10 items per page
```

### ISearchableRepository

Adds search capabilities to repositories.

```csharp
// SQL Server implementation
public async Task<ICollection<Customer>> SearchAsync(string searchTerm, string[] propertiesToSearch)
{
    var query = _context.Customers.AsQueryable();
    
    var parameter = Expression.Parameter(typeof(Customer), "c");
    var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
    
    var predicates = propertiesToSearch
        .Select(propName =>
        {
            var property = Expression.Property(parameter, propName);
            var constant = Expression.Constant(searchTerm);
            return Expression.Call(property, containsMethod, constant);
        })
        .Aggregate<Expression, Expression>(null, (current, expr) => 
            current == null ? expr : Expression.OrElse(current, expr));
    
    var lambda = Expression.Lambda<Func<Customer, bool>>(predicates, parameter);
    return await query.Where(lambda).ToListAsync();
}

// Usage
var results = await customerRepository.SearchAsync("john", new[] { "FirstName", "LastName", "Email" });
```

### IRepositoryWithBulkOperations

Adds bulk operations to repositories.

```csharp
// SQL Server implementation with EF Core
public async Task UpdateWhereAsync(Expression<Func<Customer, bool>> filter, Expression<Func<Customer, Customer>> updateExpression)
{
    await _context.Customers
        .Where(filter)
        .UpdateFromQueryAsync(updateExpression);
}

// Usage - bulk update
await customerRepository.UpdateWhereAsync(
    c => c.IsActive == false,
    c => new Customer { Status = "Inactive" });
```

## ITransactionalDataSourceContext

Extends data contexts with transaction support.

### Example Usage (SQLite)

```csharp
var context = new SqliteTransactionalDataSourceContext(connection);

await using (var transaction = await context.BeginTransactionAsync())
{
    try
    {
        // Perform operations
        var customers = context.Entities<Customer>();
        await customers.AddAsync(new Customer { /* ... */ });
        
        await context.SaveChangesAsync();
        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

### Example Usage (SQL Server)

```csharp
var context = new SqlServerTransactionalDataSourceContext(connectionString);

using (var transaction = context.BeginTransaction(IsolationLevel.Serializable))
{
    try
    {
        // Bulk operations
        var repo = new CustomerRepository(context);
        await repo.AddManyAsync(newCustomers);
        
        context.SaveChanges();
        transaction.Commit();
    }
    catch
    {
        transaction.Rollback();
        throw;
    }
}
```


The framework also supports the use of commands and batch operations being performed inside of  `IUnitOfWorkTransaction` to ensure data integrity - fail if errors encountered and ensure that data store is always in a valid state.


## Commands and Batch Operations

1. **Write Commands (`IWriteCommand`, `IWriteAsyncCommand`)**
    - Encapsulate logic for database write operations, such as insert, update, or delete.
```csharp
    internal class CreateUserCommand(UserModel user, AppDbContext dbContext): IWriteAsyncCommand
    {
       private UserModel User{get; init;} = IsValid(user) ? user : throw new ArgumentException(...);

       public Task ExecuteAsync()
       {
           dbContext.Users.Add(User);
    
       }

       private static bool IsValid(UserModel user){...}
    }
```


2. **UnitOfWorkCommandBatch (`UnitOfWorkCommandBatch`)**
    - Manages and executes queued write commands in a transactional context.
    - Ensures that changes are not persisted to the data store if one of them fails
    - supports both synchronous and asynchronous operations
    
```csharp
          private UnitOfWorkCommandBatch accountCreationBatch = new();

          public Task CreateUserProfileAndLoginAccount(User newUser, string password)
          {
             //validation checks excluded
              accountCreationBatch.QueueAsyncWriteCommand(new CreateUserCommand(newUser)); 
              accountCreationBatch.QuueueAsyncWriteCommand(new CreateLoginAccountCommand(newUser.Id, password));

              await accountCreationBatch.TryExecutesWritesAsync(this.dbContext);

           }
```

---
### **1. IWriteCommand**
Represents a database write operation.
Perfect for operations that involve some complex logic/workflow.

### Methods:
- **`Execute`** 
 Performs the write operation synchronously.

#### **1.2. IWriteAsyncCommand**
For when the operation has to be done asynchronously

#### Methods:
- **`ExecuteAsync`**
  Performs the write operation asynchronously.

#### Example:

```csharp
public class AddUserCommand : IWriteCommand
{
    private readonly User _user;
   
    public AddUserCommand(User user)
    {
       //validation logic
       _user = user;
       
    }

    public void Execute(IDataSourceContext context)
    {
        IEntityCollection<User> users = context.Entities<User>();
        if (users.Any(u=> u.Email == _user.Email)) {
            throw new DuplicateUserException(_user.Email);
        }
        users.Add(_user);
        //create accounts or some other complex logic
    }
}

public class AddUserAsyncCommand : IWriteAsyncCommand
{
    private readonly User _user;

    public AddUserCommand(User user){
    //validation logic
    _user = user;
    }

    public Task ExecuteAsync(IDataSourceContext context)
    {
        context.Users.Add(_user);
    }
}
```

---

---

## **Batching Commands**

The `TransactionalCommandExecutor` handles the execution of multiple write commands, ensuring that operations are performed within a transaction.

### **Features**
1. Queue write commands for execution.
2. Execute commands synchronously or asynchronously.
3. Automatically commit or roll back transactions based on execution success or failure.

### **Methods**

#### **QueueWriteCommand(IWriteCommand command)**
Adds a write command to the queue.

#### **QueAsyncWriteCommand(IWriteAsyncCommand command)**
Adds an async write command to the queue.

#### **TryExecuteWritesAsync(IDataSourceContext dbContext)**
Executes all queued write commands asynchronously within a transaction.

#### **TryExecuteWrites(IDataSourceContext dbContext)**
Executes all queued write commands synchronously within a transaction.


---

### **Usage Examples**

```csharp
TransactionalCommandExecutor manager = new();
var addUserCommand = new AddUserCommand(new User { Name = "Alice", IsActive = true, Email="alice@domain.sz" });
var createLoginAccount = new CreateLoginCommand(new Login{Email="alice@domain.sz", Password="pass123"} );

manager.QueueWriteCommand(addUserCommand);
manager.QueueWriteCommand(createLoginAccount);

await manager.TryExecuteWritesAsync(dataSourceContext);
```


---

### **Use Cases**
This library is designed to be used in scenarios where you value clear separation of concerns, maintainability, and the ability to perform complex data operations in a clean and efficient manner. 
It is important that we categorically mention that this library is not a replacement for Entity Framework or Dapper, but rather a complementary tool that can be used alongside them to enhance your data access layer.
Most of the abstractions here are sort of unncecessary unless of course you share the same sentiment that the data access layer should avoid magic and implicit operations, that it should be immediately clear and obvious from the method or class names what the code is doing.

The Sisusa Command Framework is designed to simplify and enhance the way developers interact with data sources. It provides a structured approach to executing commands, managing transactions, and ensuring data integrity.
The framework is particularly useful in scenarios where complex data operations are required, such as batch processing, transactional operations, and maintaining a clean separation of concerns in your codebase.

Example:
if instead of 
```cs
    var appDb = new AppDbContext();
    var transaction = appDb.Database.BeginTransaction();
    var userId = await userIdGenerator.GenerateIdAsync();
    var passwordInfo = await passwordService.HashPasswordAsync(somePassword);
    appDb.Users.Add(new User {Id=userId, Name = "Alice", IsActive = true });
    appDb.LoginAccounts.Add(new Login{Id=userId, Password=})
```
you prefer the more verbose
```cs
    var appDb = new DapperDbContext(connectionString);
    var commandChain = new TransactionalCommandExecutor();
    var userId = await userIdGenerator.GenerateIdAsync();
    var passwordInfo = await passwordService.HashPasswordAsync(somePassword);
    commandChain.QueueWriteAsyncCommand(new CreateUserCommand(userId, "Alice", Status.Active));
    commandChain.QueueWriteAsyncCommand(new CreateLoginCommand(userId, passwordInfo));

    await commandChain.TryExecuteWritesAsync(appDb);
```
or even
```cs
   class CreateUserCommand(
        IUserIdGenerator userIdGenerator,
        IPasswordService passwordService) :
        IWriteAsyncCommand
   {
        public Task ExecuteAsync(IDataSourceContext context)
        {
            var userId = await userIdGenerator.GenerateIdAsync();
            var passwordInfo = await passwordService.HashPasswordAsync(somePassword);
            context.Entities<User>()
                .Add(new User {
                                Id=userId,
                                Name = "Alice", 
                                IsActive = true 
                               });
            context.Entities<Login>()
                .Add(new Login{Id=userId, Password=}
                );
        }
   }
   //then somewhere later say in the UserService
   var appDb = new DapperDbContext(connectionString);

   await new CreateUserCommand(userIdGenerator, passwordService)
          .ExecuteAsync(appDb); //here you know user account and login are created
```
EF and Dapper already provide a lot if not better functionality that this library provides, but the Sisusa Command Framework is designed to be a lightweight and flexible solution for developers who prefer a more explicit and structured approach to data access.
Here are some common use cases:
1. **Batch Operations**  
   Combine multiple read and write commands to perform complex operations in a single transaction.

2. **Data Integrity**  
   Ensure that database changes are committed only if all operations succeed.

3. **Separation of Concerns**  
   Encapsulate database logic into reusable command objects.

4. **Asynchronous Operations**  
   Optimize performance by using asynchronous methods for data access.

---

### **Best Practices**

1. Use meaningful and descriptive names for command classes to improve code readability.
2. Always handle exceptions when executing commands to maintain data integrity.
3. Prefer asynchronous methods for better scalability in high-traffic applications.
4. Validate inputs to commands before queuing them to avoid runtime errors.
5. Ensure that you really neeed to use this over EF or Dapper, as they already provide a lot of functionality that this library provides.

---

By using the Sisusa Command Framework, developers can implement clean, maintainable, and efficient database operations while adhering to best practices in software design.