using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nbic.References.Core.Exceptions;
using Nbic.References.Core.Interfaces.Repositories;
using Nbic.References.Core.Models;
using Nbic.References.EFCore;

namespace Nbic.References.Infrastructure.Repositories
{
    public class ReferenceUsageRepository : Repository<ReferenceUsage>, IReferenceUsageRepository
    {
        public ReferenceUsageRepository(ReferencesDbContext dbContext) : base(dbContext)
        {
        }

        public Task<List<ReferenceUsage>> GetAll(int offset, int limit)
        {
            return _dbContext.ReferenceUsage.OrderBy(x => x.ReferenceId)
                .Skip(offset).Take(limit).ToListAsync();
        }

        public Task<List<ReferenceUsage>> GetFromReferenceId(Guid referenceId)
        {
            return _dbContext.ReferenceUsage.Where(x => x.ReferenceId == referenceId).ToListAsync();
        }

        public void DeleteForReference(Guid id)
        {
            var entity = _dbContext.Reference.SingleOrDefault(x=>x.Id == id);
            if (entity == null) throw new NotFoundException("Reference", id);
            var entities = _dbContext.ReferenceUsage.Where(x => x.ReferenceId == id).ToArray();
            if (entities.Length == 0) return;

            _dbContext.ReferenceUsage.RemoveRange(entities);
            _dbContext.SaveChanges();
        }

        public void DeleteUsage(Guid id, int applicationId, Guid userId)
        {
            var entities = _dbContext.ReferenceUsage.Where(x => x.ReferenceId == id && x.ApplicationId == applicationId && x.UserId == userId).ToArray();
            if (entities.Length == 0) throw new NotFoundException();

            _dbContext.ReferenceUsage.RemoveRange(entities);
            _dbContext.SaveChanges();
        }

        public async Task Add(ReferenceUsage referenceUsage)
        {
            var any = _dbContext.ReferenceUsage.Any(
                x => x.ReferenceId == referenceUsage.ReferenceId && x.ApplicationId == referenceUsage.ApplicationId
                                                        && x.UserId == referenceUsage.UserId);
            if (any) return;
            
            _dbContext.ReferenceUsage.Add(referenceUsage);

            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> AddRange(IEnumerable<ReferenceUsage> referenceUsages)
        {
            var toSave = new List<ReferenceUsage>();
            foreach (var referenceUsage in referenceUsages)
            {
                if (_dbContext.ReferenceUsage.Any(
                        x => x.ReferenceId == referenceUsage.ReferenceId && x.ApplicationId == referenceUsage.ApplicationId
                                                                         && x.UserId == referenceUsage.UserId)) continue;

                if (_dbContext.Reference.Any(x => x.Id == referenceUsage.ReferenceId))
                {
                    toSave.Add(referenceUsage);
                }
            }

            if (!toSave.Any()) return true;
            
            _dbContext.ReferenceUsage.AddRange(toSave);

            await _dbContext.SaveChangesAsync();

            return true;
        }
    }
}
