using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nbic.References.Core.Interfaces.Repositories;
using Nbic.References.Core.Models;
using Nbic.References.EFCore;

namespace Nbic.References.Infrastructure.Repositories
{
    internal class ReferenceUsageRepository : Repository<ReferenceUsage>, IReferenceUsageRepository
    {
        protected ReferenceUsageRepository(ReferencesDbContext dbContext) : base(dbContext)
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
    }
}
