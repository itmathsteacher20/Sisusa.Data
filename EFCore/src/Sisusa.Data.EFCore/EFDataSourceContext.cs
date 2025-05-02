using Microsoft.EntityFrameworkCore;
using Sisusa.Data.Contracts;
using System.Data;
using System.Data.Common;

namespace Sisusa.Data.EFCore;

/// <summary>
/// 
/// </summary>
/// <inheritdoc cref="IDataSourceContext"/>
/// <inheritdoc cref="DbContext"/>
public class EFDataSourceContext : DbContext, ITransactionalDataSourceContext
{
    //private DbConnection GetConnection()
    //{
    //    var connection = Database.GetDbConnection();
    //    if (connection.State != ConnectionState.Open)
    //    {
    //        connection.Open();
    //    }
    //    return connection;
    //}

    public IDataTransaction BeginTransaction()
    {
        var transaction = Database.BeginTransaction();
       
        return (EFDbTransaction)transaction;
    }

    public async Task<IDataTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var dbTransact  = await Database.BeginTransactionAsync(cancellationToken);

        return (new EFDbTransaction(dbTransact));
    }

    public async Task<IDataTransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.Unspecified,
        CancellationToken cancellationToken = default)
    {
        var dbTransaction = await Database.BeginTransactionAsync(isolationLevel, cancellationToken);
        return new EFDbTransaction(dbTransaction);
    }

    public IDataTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        var dbTransaction = Database.BeginTransaction(isolationLevel);
        return new EFDbTransaction(dbTransaction);
    }
}

public static class DbContextExtension
{
    public static EntitySet<TEntity> Entities<TEntity>(this DbContext context) where TEntity : class
    {
        return new EntitySet<TEntity>(context.Set<TEntity>(), context);
    }
}