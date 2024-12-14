# **Sisusa Command Framework: User Documentation**

The Sisusa Command Framework provides an interface for managing database read and write operations in a structured, transactional, and asynchronous or synchronous manner. It abstracts data operations into commands, ensuring a clean separation of concerns and improved maintainability.

---

## **Overview**

The framework revolves around three key interfaces and a central manager:

1. **Write Commands (`IWriteCommand`)**
    - Encapsulate logic for database write operations, such as insert, update, or delete.

2. **Read Commands (`IReadManyCommand<T>` and `IReadSingleCommand<T>`)**
    - Encapsulate logic for retrieving multiple or single records from the database.

3. **Data Command Manager (`DataCommandManager`)**
    - Manages and executes queued read and write commands in a transactional context.

---

## **Key Classes and Interfaces**

### **1. IWriteCommand**
Represents a database write operation.

#### Methods:
- **`Execute(IDataSourceContext context)`**  
  Performs the write operation synchronously.

- **`ExecuteAsync(IDataSourceContext context)`**  
  Performs the write operation asynchronously.

#### Example:
```csharp
public class AddUserCommand : IWriteCommand
{
    private readonly User _user;

    public AddUserCommand(User user) => _user = user;

    public void Execute(IDataSourceContext context)
    {
        context.Users.Add(_user);
        context.SaveChanges();
    }

    public async Task ExecuteAsync(IDataSourceContext context)
    {
        context.Users.Add(_user);
        await context.SaveChangesAsync();
    }
}
```

---

### **2. IReadManyCommand<T>**
Represents a database read operation that retrieves multiple records.

#### Methods:
- **`IEnumerable<T> Execute(IDataSourceContext context)`**  
  Retrieves data synchronously.

- **`Task<IEnumerable<T>> ExecuteAsync(IDataSourceContext context)`**  
  Retrieves data asynchronously.

#### Example:
```csharp
public class GetActiveUsersCommand : IReadManyCommand<User>
{
    public IEnumerable<User> Execute(IDataSourceContext context)
    {
        return context.Users.Where(u => u.IsActive).ToList();
    }

    public async Task<IEnumerable<User>> ExecuteAsync(IDataSourceContext context)
    {
        return await context.Users.Where(u => u.IsActive).ToListAsync();
    }
}
```

---

### **3. IReadSingleCommand<T>**
Represents a database read operation that retrieves a single record.

#### Methods:
- **`T? Execute(IDataSourceContext context)`**  
  Retrieves a single record synchronously.

- **`Task<T?> ExecuteAsync(IDataSourceContext context)`**  
  Retrieves a single record asynchronously.

#### Example:
```csharp
public class GetUserByIdCommand : IReadSingleCommand<User>
{
    private readonly int _userId;

    public GetUserByIdCommand(int userId) => _userId = userId;

    public User? Execute(IDataSourceContext context)
    {
        return context.Users.FirstOrDefault(u => u.Id == _userId);
    }

    public async Task<User?> ExecuteAsync(IDataSourceContext context)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Id == _userId);
    }
}
```

---

## **DataCommandManager**

The `DataCommandManager` handles the execution of multiple read and write commands, ensuring that operations are performed within a database transaction.

### **Features**
1. Queue read and write commands for execution.
2. Execute commands synchronously or asynchronously.
3. Automatically commit or roll back transactions based on execution success or failure.

### **Methods**

#### **QueueWriteCommand(IWriteCommand command)**
Adds a write command to the queue.

#### **QueueReadCommand<T>(object readCmd)**
Adds a read command to the queue. Validates that the command implements either `IReadManyCommand<T>` or `IReadSingleCommand<T>`.

#### **TryExecuteWritesAsync(IDataSourceContext dbContext)**
Executes all queued write commands asynchronously within a transaction.

#### **TryExecuteWrites(IDataSourceContext dbContext)**
Executes all queued write commands synchronously within a transaction.

#### **TryExecuteReadsAsync(IDataSourceContext dataSourceContext)**
Executes all queued read commands asynchronously and retrieves the results.

#### **TryExecuteReads(IDataSourceContext dbContext)**
Executes all queued read commands synchronously and retrieves the results.

---

## **Usage Examples**

### **1. Writing Data**
```csharp
var manager = new DataCommandManager();
var addUserCommand = new AddUserCommand(new User { Name = "Alice", IsActive = true });

manager.QueueWriteCommand(addUserCommand);

await manager.TryExecuteWritesAsync(dataSourceContext);
```

### **2. Reading Data**
```csharp
var manager = new DataCommandManager();
var getActiveUsersCommand = new GetActiveUsersCommand();

manager.QueueReadCommand<IEnumerable<User>>(getActiveUsersCommand);

var results = await manager.TryExecuteReadsAsync(dataSourceContext);
foreach (var user in results)
{
    Console.WriteLine(user.Name);
}
```

### **3. Mixed Read and Write Operations**
```csharp
var manager = new DataCommandManager();

var addUserCommand = new AddUserCommand(new User { Name = "Bob", IsActive = false });
var getActiveUsersCommand = new GetActiveUsersCommand();

manager.QueueWriteCommand(addUserCommand);
manager.QueueReadCommand<IEnumerable<User>>(getActiveUsersCommand);

await manager.TryExecuteWritesAsync(dataSourceContext);
var results = await manager.TryExecuteReadsAsync(dataSourceContext);

foreach (var user in results)
{
    Console.WriteLine(user.Name);
}
```

---

## **Use Cases**

1. **Batch Operations**  
   Combine multiple read and write commands to perform complex operations in a single transaction.

2. **Data Integrity**  
   Ensure that database changes are committed only if all operations succeed.

3. **Separation of Concerns**  
   Encapsulate database logic into reusable command objects.

4. **Asynchronous Operations**  
   Optimize performance by using asynchronous methods for data access.

---

## **Best Practices**

1. Use meaningful and descriptive names for command classes to improve code readability.
2. Always handle exceptions when executing commands to maintain data integrity.
3. Prefer asynchronous methods for better scalability in high-traffic applications.
4. Validate inputs to commands before queuing them to avoid runtime errors.

---

By using the Sisusa Command Framework, developers can implement clean, maintainable, and efficient database operations while adhering to best practices in software design.