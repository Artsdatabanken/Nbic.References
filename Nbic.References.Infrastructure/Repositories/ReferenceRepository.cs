using Microsoft.EntityFrameworkCore;
using Nbic.References.Core.Exceptions;
using Nbic.References.Core.Interfaces.Repositories;
using Nbic.References.Core.Models;
using Nbic.References.Infrastructure.Repositories.DbContext;
using Index = Nbic.References.Infrastructure.Services.Indexing.Index;

namespace Nbic.References.Infrastructure.Repositories;

public class ReferenceRepository : Repository<Reference>, IReferencesRepository
{
    private readonly Index _index;
    private readonly DbSet<Reference> _references;

    public ReferenceRepository(ReferencesDbContext dbContext, Index index) : base(dbContext)
    {
        _index = index;
        _references = _dbContext.Set<Reference>();
    }

    //public Task<int> CountAsync()
    //{
    //    return dbContext.Reference.CountAsync();
    //}

    public Task<Reference?> Get(Guid id)
    {
        return _references.Include(x => x.ReferenceUsage)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public void Delete(Guid id)
    {
        var item = _references.Include(x => x.ReferenceUsage).FirstOrDefault(x => x.Id == id);
        if (item == null) throw new NotFoundException("Reference", id);
        if (item.ReferenceUsage.Any())
        {
            throw new InvalidOperationException("Can not delete reference with referenceusages. Remove them first.");
        }

        _references.Remove(item);
        _dbContext.SaveChanges();
        _index.Delete(item.Id);
    }

    public async Task<Reference> Add(Reference reference)
    {
        if (reference.Id == Guid.Empty) reference.Id = Guid.NewGuid();

        try
        {
            _references.Add(reference);
            await _dbContext.SaveChangesAsync();
            _index.AddOrUpdate(reference);
        }
        catch (DbUpdateException e)
        {
            if (e.Message.Contains("Violation of PRIMARY KEY constraint", StringComparison.InvariantCulture))
            {
                throw new BadRequestException("Violation of PRIMARY KEY constraint. Key exists!");
            }

            throw;
        }

        return reference;
    }

    public async Task AddRange(Reference[] references)
    {
        foreach (var reference in references)
        {
            if (reference.Id == Guid.Empty)
            {
                reference.Id = Guid.NewGuid();
            }
        }

        try
        {
            await _references.AddRangeAsync(references);
            await _dbContext.SaveChangesAsync();
            _index.AddOrUpdate(references);
        }
        catch (DbUpdateException e)
        {
            if (e.Message.Contains("Violation of PRIMARY KEY constraint", StringComparison.InvariantCulture))
            {
                throw new BadRequestException("Violation of PRIMARY KEY constraint. Key exists!");
            }

            throw;
        }

    }

    public async Task Update(Reference reference)
    {
         var r = await _references.Include(x => x.ReferenceUsage).FirstOrDefaultAsync(x => x.Id == reference.Id);
            if (r == null)
            {
                throw new NotFoundException("Reference", reference.Id);
            }
            
            // transfer values
            // these first three should never be changed
            //if (r.PkReferenceId != value.PkReferenceId) r.PkReferenceId = value.PkReferenceId;
            if (r.ApplicationId != reference.ApplicationId) r.ApplicationId = reference.ApplicationId;
            if (r.UserId != reference.UserId) r.UserId = reference.UserId;
            if (r.Author != reference.Author) r.Author = reference.Author;
            if (r.Bibliography != reference.Bibliography) r.Bibliography = reference.Bibliography;
            if (r.Firstname != reference.Firstname) r.Firstname = reference.Firstname;
            if (r.ReferenceString != reference.ReferenceString) r.ReferenceString = reference.ReferenceString;
            if (r.Journal != reference.Journal) r.Journal = reference.Journal;
            if (r.Keywords != reference.Keywords) r.Keywords = reference.Keywords;
            if (r.Lastname != reference.Lastname) r.Lastname = reference.Lastname;
            if (r.Middlename != reference.Middlename) r.Middlename = reference.Middlename;
            if (r.Pages != reference.Pages) r.Pages = reference.Pages;
            if (r.Summary != reference.Summary) r.Summary = reference.Summary;
            if (r.Url != reference.Url) r.Url = reference.Url;
            if (r.Title != reference.Title) r.Title = reference.Title;
            if (r.Volume != reference.Volume) r.Volume = reference.Volume;
            if (r.Year != reference.Year) r.Year = reference.Year;
            if (reference.ReferenceUsage != null && reference.ReferenceUsage.Any())
            {
                r.ReferenceUsage.Clear();
                foreach (var item in reference.ReferenceUsage)
                {
                    item.Reference = r;
                    item.ReferenceId = r.Id;
                    r.ReferenceUsage.Add(item);
                }
            }

            r.EditDate = DateTime.Now;
            
            // todo usages
            await _dbContext.SaveChangesAsync();
            _index.AddOrUpdate(r);
    }

    public async Task<List<Reference>> Search(string search, int offset, int limit)
    {
        if (string.IsNullOrWhiteSpace(search))
            return await _references.Include(x => x.ReferenceUsage).OrderBy(x => x.Id)
                .Skip(offset).Take(limit).ToListAsync();

        var searchresults = _index.SearchReference(search, offset, limit);
        var guids = searchresults as Guid[] ?? searchresults.ToArray();
        if (!guids.Any())
        {
            return new List<Reference>();
        }

        return await _references.Include(x => x.ReferenceUsage).Where(x => guids.Contains(x.Id))
            .OrderBy(x => x.Id).Take(limit).ToListAsync();
    }

    public void ReIndex()
    {

        if (!_index.FirstUse) return;

        if (_index.IndexCount() != _references.Count())
        {
            _index.FirstUse = false;
            _index.ClearIndex();
            var batch = new List<Reference>();
            foreach (var reference in _references)
            {
                batch.Add(reference);
                if (batch.Count <= 20) continue;

                _index.AddOrUpdate(batch);
                batch = new List<Reference>();
            }

            if (batch.Count > 0)
            {
                _index.AddOrUpdate(batch);
            }
        }

        _index.FirstUse = false;
    }

}