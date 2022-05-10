using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nbic.References.Core.Models;

namespace Nbic.References.Core.Interfaces.Repositories
{
    public interface IReferenceUsageRepository : IRepository<ReferenceUsage>
    {
        Task<List<ReferenceUsage>> GetAll(int offset, int limit);
        Task<List<ReferenceUsage>> GetFromReferenceId(Guid referenceId);
        void DeleteForReference(Guid id);
        void DeleteUsage(Guid id, int applicationId, Guid userId);
        Task Add(ReferenceUsage referenceUsage);
        Task<bool> AddRange(ReferenceUsage[] referenceUsages);
    }
}
