using Nbic.References.Core.Models;

namespace Nbic.References.Core.Interfaces.Repositories;

public interface IReferenceUsageRepository : IRepository<ReferenceUsage>
{
    Task<List<ReferenceUsage>> GetAll(int offset, int limit);
    Task<List<ReferenceUsage>> GetFromReferenceId(Guid referenceId);
    void DeleteForReference(Guid id);
    void DeleteUsage(Guid id, int applicationId, Guid userId);
    Task Add(ReferenceUsage referenceUsage);
    Task<bool> AddRange(IEnumerable<ReferenceUsage> referenceUsages);
}