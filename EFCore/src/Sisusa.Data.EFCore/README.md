# User Documentation for Repository Pattern Implementation in `Sisusa.Data.EFCore`

This document provides a comprehensive guide for using the `SimpleRepository`, `BaseRepository`, and new query-related classes from the `Sisusa.Data.EFCore` namespace. These classes implement the repository pattern for managing data access in applications using Entity Framework Core.

---

## Overview

### Purpose
The repository pattern abstracts data access logic, providing a consistent and reusable interface for performing CRUD operations on database entities. This implementation supports generic data access for entities identified by a primary key (`TId`).

### Key Components
1. **`SimpleRepository`**
    - Provides basic CRUD and query operations.
    - Uses `EntitySet` to interact with `DbSet`.

2. **`BaseRepository`**
    - Extends `SimpleRepository` with additional functionality like managing proxy entities.
    - Supports creation of proxy objects using a `Func<TId, TEntity>` delegate.

3. **`DataSourceContext`**
    - Extends `DbContext` to manage entity sets (`EntitySet<T>`).
    - Supports transaction management.

4. **`Query<TEntity>`**
    - Provides a fluent interface for building queries with filtering, ordering, and projections.

5. **`EntitySet<T>`**
    - An implementation of `IEntityCollection<T>` for use with an EF context, enabling advanced querying and entity management.

---

## Prerequisites

### Installation
Ensure you have the following packages installed:
- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.SqlServer (or other database provider)

### Setup
Configure the `DataSourceContext` in your `Startup.cs` or `Program.cs` file:
```csharp
services.AddDbContext<DataSourceContext>(options =>
    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

services.AddScoped(typeof(IRepository<,>), typeof(SimpleRepository<,>));
services.AddScoped(typeof(BaseRepository<,>));
```

---

## Class Descriptions

### `SimpleRepository`
The `SimpleRepository` class provides generic CRUD operations for entities.

#### Key Methods

- **AddNewAsync**
  ```csharp
  public virtual Task AddNewAsync(TEntity entity)
  ```
  Adds a new entity to the database.

- **FindAllAsync**
  ```csharp
  public virtual Task<List<TEntity>> FindAllAsync()
  ```
  Retrieves all entities from the database.

- **FindByIdAsync**
  ```csharp
  public virtual Task<TEntity?> FindByIdAsync(TId id)
  ```
  Retrieves an entity by its primary key.

- **UpdateAsync**
  ```csharp
  public virtual Task UpdateAsync(TEntity entity)
  ```
  Updates an existing entity in the database.

- **DeleteByIdAsync**
  ```csharp
  public virtual Task DeleteByIdAsync(TId id)
  ```
  Deletes an entity by its primary key.

### `BaseRepository`
The `BaseRepository` class extends `SimpleRepository` to include proxy creation and additional entity management capabilities.

#### Key Methods

- **AddNewAsync**
  Behaves the same as in `SimpleRepository` but supports additional logic for proxies.

- **UpdateByIdAsync**
  ```csharp
  public virtual Task UpdateByIdAsync(TId id, TEntity updatedEntity)
  ```
  Updates an entity identified by its primary key.

- **HasByIdAsync**
  ```csharp
  public virtual Task<bool> HasByIdAsync(TId id)
  ```
  Checks if an entity exists by its primary key.

### `Query<TEntity>`
The `Query<TEntity>` class provides a fluent API for building queries with filtering, ordering, and projections.

#### Key Methods

- **Where**
  ```csharp
  public Query<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
  ```
  Adds a filtering condition to the query.

- **OrderBy**
  ```csharp
  public Query<TEntity> OrderBy(Expression<Func<TEntity, object>> keySelector, OrderMode mode = OrderMode.Ascending)
  ```
  Adds an ordering to the query.

- **ThenBy**
  ```csharp
  public Query<TEntity> ThenBy(Expression<Func<TEntity, object>> keySelector, OrderMode mode = OrderMode.Ascending)
  ```
  Adds a secondary ordering to the query.

- **Include**
  ```csharp
  public Query<TEntity> Include(Expression<Func<TEntity, object>> navigationPropertyPath)
  ```
  Includes a navigation property in the query.

- **ProjectTo**
  ```csharp
  public IQueryable<TNew> ProjectTo<TNew>(Expression<Func<TEntity, TNew>> selector) where TNew : class
  ```
  Projects the query results to a new type.

### `EntitySet<T>`
The `EntitySet<T>` class provides advanced querying and entity management capabilities.

#### Key Methods

- **AddAsync**
  ```csharp
  public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
  ```
  Adds a new entity to the set.

- **RemoveAsync**
  ```csharp
  public async Task RemoveAsync(T entity, CancellationToken cancellationToken = default)
  ```
  Removes an entity from the set.

- **CountAsync**
  ```csharp
  public async Task<int> CountAsync()
  ```
  Returns the count of entities in the set.

---

## Usage Examples

### 1. `SimpleRepository`: Basic CRUD Operations

#### Scenario
Suppose we have an `Employee` entity with a primary key of type `int`. We need to manage employees using the repository.

#### Setup
```csharp
public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Department { get; set; }
}

var repository = new SimpleRepository<Employee, int>(context);
```

#### Examples

- **Add a New Employee**
  ```csharp
  var newEmployee = new Employee { Name = "John Doe", Department = "HR" };
  await repository.AddNewAsync(newEmployee);
  ```

- **Retrieve All Employees**
  ```csharp
  var employees = await repository.FindAllAsync();
  ```

- **Retrieve an Employee by ID**
  ```csharp
  var employee = await repository.FindByIdAsync(1);
  ```

- **Update an Employee**
  ```csharp
  var employee = await repository.FindByIdAsync(1);
  if (employee != null)
  {
      employee.Department = "Finance";
      await repository.UpdateAsync(employee);
  }
  ```

- **Delete an Employee**
  ```csharp
  await repository.DeleteByIdAsync(1);
  ```

### 2. `BaseRepository`: Advanced Operations with Proxy Creation

#### Scenario
Suppose we have a `Product` entity. The `BaseRepository` allows efficient handling of proxy objects for managing data.

#### Setup
```csharp
public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

var repository = new BaseRepository<Product, Guid>(context, id => new Product { Id = id });
```

#### Examples

- **Add a New Product**
  ```csharp
  var product = new Product { Id = Guid.NewGuid(), Name = "Laptop", Price = 1200.00m };
  await repository.AddNewAsync(product);
  ```

- **Update a Product by ID**
  ```csharp
  var productId = Guid.NewGuid();
  var updatedProduct = new Product { Name = "Gaming Laptop", Price = 1500.00m };
  await repository.UpdateByIdAsync(productId, updatedProduct);
  ```

- **Delete a Product by ID**
  ```csharp
  var productId = Guid.NewGuid();
  await repository.DeleteByIdAsync(productId);
  ```

- **Check if a Product Exists**
  ```csharp
  var exists = await repository.HasByIdAsync(productId);
  ```

### 3. `Query<TEntity>`: Building Queries

#### Scenario
Using the `Query<TEntity>` class to filter and order results.

#### Setup
```csharp
var query = Query<Product>.FromContext(context)
    .Where(p => p.Price > 1000)
    .OrderBy(p => p.Name);
```

#### Example
- **Get Products**
  ```csharp
  var products = await query.GetCompiled().ToListAsync();
  ```

### 4. Transaction Management with `DataSourceContext`

#### Scenario
You want to ensure atomicity when performing multiple operations.

#### Example
```csharp
using var transaction = await context.BeginTransactionAsync();
try
{
    await repository.AddNewAsync(new Employee { Name = "Alice", Department = "IT" });
    await repository.AddNewAsync(new Employee { Name = "Bob", Department = "Finance" });

    await context.SaveChangesAsync();
    transaction.Commit();
}
catch
{
    transaction.Rollback();
    throw;
}
```

---

## Best Practices

1. **Validation**
    - Ensure inputs (e.g., `id` and `entity`) are not null.

2. **Error Handling**
    - Handle `EntityNotFoundException` gracefully when performing updates or deletions.

3. **Transaction Usage**
    - Wrap multiple related operations in a transaction to maintain consistency.

4. **Entity Set Configuration**
    - Customize `EntitySet` logic to suit specific data access requirements.

---

## Conclusion
The `SimpleRepository`, `BaseRepository`, `Query<TEntity>`, and `EntitySet<T>` classes provide powerful abstractions for managing database operations. Their extensible design supports a variety of use cases, making data access straightforward and maintainable. Use these classes to simplify your data layer and focus on core business logic.
