using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Nbic.References.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.EntityFrameworkCore;

    using EFCore;
    using Public.Models;

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
            return await _referencesDbContext.ReferenceUsage.OrderBy(x => x.ReferenceId)
                       .Skip(offset).Take(limit).ToListAsync().ConfigureAwait(false);
        }
        [HttpGet]
        [Route("Reference/{id}")]
        public async Task<List<ReferenceUsage>> Get(Guid id)
        {
            return await _referencesDbContext.ReferenceUsage.Where(x=>x.ReferenceId == id).ToListAsync().ConfigureAwait(false);
        }

        [HttpGet]
        [Route("Count")]
        public async Task<ActionResult<int>> GetCount()
        {
            return await _referencesDbContext.ReferenceUsage.CountAsync().ConfigureAwait(false);
        }

        [Authorize("WriteAccess")]
        [HttpDelete("{id}")]
        public ActionResult DeleteAllUsages(Guid id)
        {
            var entities = _referencesDbContext.ReferenceUsage.Where(x => x.ReferenceId == id).ToArray();
            if (entities.Length == 0) return NotFound();
            
            _referencesDbContext.ReferenceUsage.RemoveRange(entities);
            _referencesDbContext.SaveChanges();
            return Ok();

        }
        [Authorize("WriteAccess")]
        [HttpDelete("{id},{applicationId},{userId}")]
        public ActionResult DeleteUsage(Guid id, int applicationId, Guid userId)
        {
            var entities = _referencesDbContext.ReferenceUsage.Where(x => x.ReferenceId == id && x.ApplicationId == applicationId && x.UserId == userId).ToArray();
            if (entities.Length == 0) return NotFound();

            _referencesDbContext.ReferenceUsage.RemoveRange(entities);
            _referencesDbContext.SaveChanges();
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

            if (_referencesDbContext.ReferenceUsage.Any(
                x => x.ReferenceId == value.ReferenceId && x.ApplicationId == value.ApplicationId
                                                        && x.UserId == value.UserId)) return value;
            
            _referencesDbContext.ReferenceUsage.Add(value);

            await _referencesDbContext.SaveChangesAsync().ConfigureAwait(false);

            return value;
        }

        [Authorize("WriteAccess")]
        [HttpPost("bulk")]
        public async Task<ActionResult<bool>> Post(ReferenceUsage[] value)
        {
            if (value == null)
            {
                return BadRequest("No data posted");
            }

            var toSave = new List<ReferenceUsage>();
            foreach (var referenceUsage in value)
            {
                if (_referencesDbContext.ReferenceUsage.Any(
                    x => x.ReferenceId == referenceUsage.ReferenceId && x.ApplicationId == referenceUsage.ApplicationId
                                                                     && x.UserId == referenceUsage.UserId)) continue;

                if (_referencesDbContext.Reference.Any(x => x.Id == referenceUsage.ReferenceId))
                {
                    toSave.Add(referenceUsage);
                }

            }

            if (!toSave.Any()) return true;
            
            _referencesDbContext.ReferenceUsage.AddRange(toSave);

            await _referencesDbContext.SaveChangesAsync().ConfigureAwait(false);


            return true;
        }
    }
}