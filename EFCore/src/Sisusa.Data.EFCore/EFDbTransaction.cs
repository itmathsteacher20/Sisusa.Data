using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using Sisusa.Data.Contracts;

namespace Sisusa.Data.EFCore
{
    public class EFDbTransaction : IDataTransaction
    {
        public Guid TransactionId => new();

        private IDbContextTransaction _dbContextTransaction;

        public EFDbTransaction(IDbContextTransaction transactionContext)
        {
            ArgumentNullException.ThrowIfNull(transactionContext);
            _dbContextTransaction = transactionContext;
        }

        public void Commit()
        {
            _dbContextTransaction.Commit();
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            return _dbContextTransaction.CommitAsync(cancellationToken);
        }

        public void Dispose()
        {
            _dbContextTransaction.Dispose();
        }

        public ValueTask DisposeAsync()
        {
            return _dbContextTransaction.DisposeAsync();
        }

        public void Rollback()
        {
            _dbContextTransaction.Rollback();
        }

        public Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            return _dbContextTransaction.RollbackAsync(cancellationToken);
        }

        public static implicit operator RelationalTransaction(EFDbTransaction efTransact)
        {
            return (RelationalTransaction)efTransact._dbContextTransaction;
        }

        public static implicit operator EFDbTransaction(RelationalTransaction efTransact)
        {
            return new(efTransact);
        }
    }
}
