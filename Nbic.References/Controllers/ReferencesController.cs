using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nbic.References.EFCore;
using Nbic.References.Public.Models;

namespace Nbic.References.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReferencesController : ControllerBase
    {
        private readonly ReferencesDbContext _referencesDbContext;

        public ReferencesController(ReferencesDbContext referencesDbContext)
        {
            _referencesDbContext = referencesDbContext;
        }

        [HttpGet]
        public async Task<List<Reference>> GetAll(int offset = 0, int limit = 10)
        {
            return await this._referencesDbContext.Reference.Include(x => x.ReferenceUsage).OrderBy(x => x.Id)
                       .Skip(offset).Take(limit).ToListAsync().ConfigureAwait(false);
        }

        [HttpGet]
        [Route("Count")]
        public async Task<ActionResult<int>> GetCount()
        {
            return await this._referencesDbContext.Reference.CountAsync().ConfigureAwait(false);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Reference>> Get(Guid id)
        {
            var reference = await _referencesDbContext.Reference.Include(x => x.ReferenceUsage)
                                .FirstOrDefaultAsync(x => x.Id == id).ConfigureAwait(false);
            if (reference == null)
            {
                return NotFound();
            }

            return reference;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Reference>> PostAsync([FromBody] Reference value)
        {
            if (value == null)
            {
                return BadRequest("No data posted");
            }

            if (value.Id == Guid.Empty)
            {
                value.Id = Guid.NewGuid();
            }

            _referencesDbContext.Reference.Add(value);
            try
            {
                await this._referencesDbContext.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (SqlException e)
            {
                if (e.Message.Contains("Violation of PRIMARY KEY constraint", StringComparison.InvariantCulture))
                {
                    return BadRequest("Violation of PRIMARY KEY constraint. Key exists!");
                }
            }
        
            return value;
        }
        [Authorize]
        [HttpPost]
        [Route("Bulk")]
        public ActionResult PostMany([FromBody] Reference[] values)
        {
            if (values == null || values.Length == 0)
            {
                return BadRequest("No data posted");
            }

            foreach (var value in values)
            {
                if (value.Id == Guid.Empty)
                {
                    value.Id = Guid.NewGuid();
                }
            }
            
            _referencesDbContext.Reference.AddRange(values);
            try
            {
                var recordsSaved = _referencesDbContext.SaveChanges();
            }
            catch (SqlException e)
            {
                if (e.Message.Contains("Violation of PRIMARY KEY constraint", StringComparison.InvariantCulture))
                {
                    return BadRequest("Violation of PRIMARY KEY constraint. Key exists!");
                }
            }
            return Ok();
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(Guid id, [FromBody] Reference value)
        {
            if (value == null)
            {
                return BadRequest("No reference to put");
            }

            var r = await this._referencesDbContext.Reference.Include(x => x.ReferenceUsage).FirstOrDefaultAsync(x => x.Id == id).ConfigureAwait(false);
            if (r == null)
            {
                return NotFound();
            }
            // transfer values
            // these first three should never be changed
            //if (r.PkReferenceId != value.PkReferenceId) r.PkReferenceId = value.PkReferenceId;
            if (r.ApplicationId != value.ApplicationId) r.ApplicationId = value.ApplicationId;
            if (r.UserId != value.UserId) r.UserId = value.UserId;
            if (r.Author != value.Author) r.Author = value.Author;
            if (r.Bibliography != value.Bibliography) r.Bibliography = value.Bibliography;
            if (r.Firstname != value.Firstname) r.Firstname = value.Firstname;
            if (r.ImportXml != value.ImportXml) r.ImportXml = value.ImportXml;
            if (r.Journal != value.Journal) r.Journal = value.Journal;
            if (r.Keywords != value.Keywords) r.Keywords = value.Keywords;
            if (r.Lastname != value.Lastname) r.Lastname = value.Lastname;
            if (r.Middlename != value.Middlename) r.Middlename = value.Middlename;
            if (r.Pages != value.Pages) r.Pages = value.Pages;
            if (r.Summary != value.Summary) r.Summary = value.Summary;
            if (r.Url != value.Url) r.Url = value.Url;
            if (r.Title != value.Title) r.Title = value.Title;
            if (r.Volume != value.Volume) r.Volume = value.Volume;
            if (r.Year != value.Year) r.Year = value.Year;
            if (value.ReferenceUsage.Any())
            {
                r.ReferenceUsage.Clear();
                foreach (var item in value.ReferenceUsage)
                {
                    item.Reference = r;
                    item.ReferenceId = r.Id;
                    r.ReferenceUsage.Add(item);
                }
            }

            r.EditDate = DateTime.Now;
            
            // todo usages

            await this._referencesDbContext.SaveChangesAsync().ConfigureAwait(false);
            return Ok();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public ActionResult Delete(Guid id)
        {
            var item = this._referencesDbContext.Reference.Include(x => x.ReferenceUsage).FirstOrDefault(x => x.Id == id);
            if (item == null) return NotFound();
            if (item.ReferenceUsage.Any())
            {
                throw  new InvalidOperationException("Can not delete reference with referenceusages. Remove them first.");
            }
            _referencesDbContext.Reference.Remove(item);
            this._referencesDbContext.SaveChanges();
            return Ok();

        }
    }
}
