using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Sisusa.Data.Contracts;

namespace Sisusa.Data.ContractsTests
{
    public class PeopleCollection : IEntityCollection<Person>
    {
        private IAsyncEnumerable<Person> _people = new Async;
        public Task AddAsync(Person entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task AddRangeAsync(IEnumerable<Person> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AnyAsync(Expression<Func<Person, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AnyAsync()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Person> AsEnumerable()
        {
            throw new NotImplementedException();
        }

        public int Count()
        {
            throw new NotImplementedException();
        }

        public Task<int> CountAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Person?> FindAsync(params object[] keyValues)
        {
            throw new NotImplementedException();
        }

        public Task<Person> FirstAsync(Expression<Func<Person, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<Person> FirstAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Person?> FirstOrDefaultAsync(Expression<Func<Person, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<Person?> FirstOrDefaultAsync()
        {
            throw new NotImplementedException();
        }

        public long LongCount()
        {
            throw new NotImplementedException();
        }

        public Task<long> LongCountAsync()
        {
            throw new NotImplementedException();
        }

        public void Remove(Person entity)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(Person entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void RemoveRange(IEnumerable<Person> entities)
        {
            throw new NotImplementedException();
        }

        public Task RemoveRangeAsync(IEnumerable<Person> entities)
        {
            throw new NotImplementedException();
        }

        public Task<Person> SingleAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<Person>> ToListAsync()
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Person entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Person> Where(Expression<Func<Person, bool>> predicate)
        {
            throw new NotImplementedException();
        }
    }

    public class PersonFileStore : IDataSourceContext
    {
        private readonly string _fileName;

        private readonly List<Person> _people = [];

        
        public IEntityCollection<T> Entities<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public int SaveChanges()
        {
            throw new NotImplementedException();
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
