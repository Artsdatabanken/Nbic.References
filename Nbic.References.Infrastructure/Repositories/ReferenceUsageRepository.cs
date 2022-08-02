using Microsoft.EntityFrameworkCore;
using Nbic.References.Core.Exceptions;
using Nbic.References.Core.Interfaces.Repositories;
using Nbic.References.Core.Models;
using Nbic.References.Infrastructure.Repositories.DbContext;

namespace Nbic.References.Infrastructure.Repositories
{
    public class ReferenceUsageRepository : Repository<ReferenceUsage>, IReferenceUsageRepository
    {
        private readonly DbSet<ReferenceUsage> _referenceUsages;
        private readonly DbSet<Reference> _references;

        public ReferenceUsageRepository(ReferencesDbContext dbContext) : base(dbContext)
        {
            _referenceUsages = _dbContext.Set<ReferenceUsage>();
            _references = _dbContext.Set<Reference>();
        }

        public Task<List<ReferenceUsage>> GetAll(int offset, int limit)
        {
            return _referenceUsages.OrderBy(x => x.ReferenceId)
                .Skip(offset).Take(limit).ToListAsync();
        }

        public Task<List<ReferenceUsage>> GetFromReferenceId(Guid referenceId)
        {
            return _referenceUsages.Where(x => x.ReferenceId == referenceId).ToListAsync();
        }

        public void DeleteForReference(Guid id)
        {
            var entity = _references.SingleOrDefault(x=>x.Id == id);
            if (entity == null) throw new NotFoundException("Reference", id);
            var entities = _referenceUsages.Where(x => x.ReferenceId == id).ToArray();
            if (entities.Length == 0) return;

            _referenceUsages.RemoveRange(entities);
            _dbContext.SaveChanges();
        }

        public void DeleteUsage(Guid id, int applicationId, Guid userId)
        {
            var entities = _referenceUsages.Where(x => x.ReferenceId == id && x.ApplicationId == applicationId && x.UserId == userId).ToArray();
            if (entities.Length == 0) throw new NotFoundException();

            _referenceUsages.RemoveRange(entities);
            _dbContext.SaveChanges();
        }

        public async Task Add(ReferenceUsage referenceUsage)
        {
            var any = _referenceUsages.Any(
                x => x.ReferenceId == referenceUsage.ReferenceId && x.ApplicationId == referenceUsage.ApplicationId
                                                        && x.UserId == referenceUsage.UserId);
            if (any) return;
            
            _referenceUsages.Add(referenceUsage);

            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> AddRange(ReferenceUsage[] referenceUsages)
        {
            var toSave = new List<ReferenceUsage>();
            foreach (var referenceUsage in referenceUsages)
            {
                if (_referenceUsages.Any(
                        x => x.ReferenceId == referenceUsage.ReferenceId && x.ApplicationId == referenceUsage.ApplicationId
                                                                         && x.UserId == referenceUsage.UserId)) continue;

                if (_references.Any(x => x.Id == referenceUsage.ReferenceId))
                {
                    toSave.Add(referenceUsage);
                }
            }

            if (!toSave.Any()) return true;
            
            _referenceUsages.AddRange(toSave);

            await _dbContext.SaveChangesAsync();

            return true;
        }
    }
}
