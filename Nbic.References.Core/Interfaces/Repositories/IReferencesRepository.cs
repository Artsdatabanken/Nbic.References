using Nbic.References.Core.Models;

namespace Nbic.References.Core.Interfaces.Repositories;

public interface IReferencesRepository : IRepository<Reference>
{
    Task<Reference?> Get(Guid id);
    void Delete(Guid id);
    Task<Reference> Add(Reference reference);
    void ReIndex();
    Task AddRange(Reference[] references);
    Task Update(Reference reference);
    Task<List<Reference>> Search(string search, int offset, int limit);
}