using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Nbic.References.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.EntityFrameworkCore;

    using Nbic.References.EFCore;
    using Nbic.References.Public.Models;

    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Read, add or delete ReferencesUsages")]
    public class ReferenceUsageController : ControllerBase
    {
        private readonly ReferencesDbContext _referencesDbContext;

        public ReferenceUsageController(ReferencesDbContext referencesDbContext)
        {
            _referencesDbContext = referencesDbContext;
        }

        [HttpGet]
        public async Task<List<ReferenceUsage>> GetAll(int offset = 0, int limit = 10)
        {
            return await this._referencesDbContext.ReferenceUsage.OrderBy(x => x.ReferenceId)
                       .Skip(offset).Take(limit).ToListAsync().ConfigureAwait(false);
        }
        [HttpGet]
        [Route("Reference/{id}")]
        public async Task<List<ReferenceUsage>> Get(Guid id)
        {
            return await this._referencesDbContext.ReferenceUsage.Where(x=>x.ReferenceId == id).ToListAsync().ConfigureAwait(false);
        }

        [HttpGet]
        [Route("Count")]
        public async Task<ActionResult<int>> GetCount()
        {
            return await this._referencesDbContext.ReferenceUsage.CountAsync().ConfigureAwait(false);
        }

        [Authorize("WriteAccess")]
        [HttpDelete("{id}")]
        public ActionResult DeleteAllUsages(Guid id)
        {
            var entities = this._referencesDbContext.ReferenceUsage.Where(x => x.ReferenceId == id).ToArray();
            if (entities.Length == 0) return NotFound();
            
            _referencesDbContext.ReferenceUsage.RemoveRange(entities);
            this._referencesDbContext.SaveChanges();
            return Ok();

        }
        [Authorize("WriteAccess")]
        [HttpDelete("{id},{applicationId},{userId}")]
        public ActionResult DeleteUsage(Guid id, int applicationId, int userId)
        {
            var entities = this._referencesDbContext.ReferenceUsage.Where(x => x.ReferenceId == id && x.ApplicationId == applicationId && x.UserId == userId).ToArray();
            if (entities.Length == 0) return NotFound();

            _referencesDbContext.ReferenceUsage.RemoveRange(entities);
            this._referencesDbContext.SaveChanges();
            return Ok();

        }

        [Authorize("WriteAccess")]
        [HttpPost]
        public async Task<ActionResult<ReferenceUsage>> Post([FromBody] ReferenceUsage value)
        {
            if (value == null)
            {
                return BadRequest("No data posted");
            }

            if (!_referencesDbContext.ReferenceUsage.Any(
                    x => x.ReferenceId == value.ReferenceId && x.ApplicationId == value.ApplicationId
                                                            && x.UserId == value.UserId))
            {
                _referencesDbContext.ReferenceUsage.Add(value);

                await this._referencesDbContext.SaveChangesAsync().ConfigureAwait(false);
            }


            return value;
        }
    }
}