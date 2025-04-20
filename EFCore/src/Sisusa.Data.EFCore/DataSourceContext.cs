using Microsoft.EntityFrameworkCore;
using Sisusa.Data.Contracts;
using System.Data;

namespace Sisusa.Data.EFCore;

/// <summary>
/// 
/// </summary>
/// <inheritdoc cref="IDataSourceContext"/>
/// <inheritdoc cref="DbContext"/>
public class DataSourceContext : DbContext, IDataSourceContext
{

    public IDbTransaction BeginTransaction()
    {
        var transaction = Database.BeginTransaction();
        return (IDbTransaction)transaction;
    }

    public async Task<IDbTransaction> BeginTransactionAsync()
    {
        return (IDbTransaction)(await Database.BeginTransactionAsync()); //as IDbTransaction;
    }
}

public static class DbContextExtension
{
    public static EntitySet<TEntity> Entities<TEntity>(this DbContext context) where TEntity : class
    {
        return new EntitySet<TEntity>(context.Set<TEntity>(), context);
    }
}