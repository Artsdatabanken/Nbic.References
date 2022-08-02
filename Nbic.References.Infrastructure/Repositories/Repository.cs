using Microsoft.EntityFrameworkCore;
using Nbic.References.Core.Interfaces.Repositories;
using Nbic.References.Infrastructure.Repositories.DbContext;

namespace Nbic.References.Infrastructure.Repositories
{
    public abstract class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ReferencesDbContext _dbContext;
        private readonly DbSet<T> _dbSet;

        protected Repository(ReferencesDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
        }

        public int Count()
        {
            return _dbSet.Count();
        }

        public Task<int> CountAsync()
        {
            return _dbSet.CountAsync();
        }
    }
}
