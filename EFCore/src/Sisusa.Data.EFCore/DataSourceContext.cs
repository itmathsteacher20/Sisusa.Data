using Microsoft.EntityFrameworkCore;
using Sisusa.Data.Contracts;
using System.Data;

namespace Sisusa.Data.EFCore;

public class DataSourceContext : DbContext, IDataSourceContext
{

    public EntitySet<T> Entities<T>() where T : class
    {
        return new EntitySet<T>(Set<T>(), this);
    }

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