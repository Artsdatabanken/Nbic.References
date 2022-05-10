using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Nbic.References.Controllers
{
    using Microsoft.AspNetCore.Authorization;

    using EFCore;

    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Read, add or delete ReferencesUsages")]
    public class ReferenceUsageController : ControllerBase
    {
        private readonly IReferenceUsageRepository _referenceUsageRepository;

        public ReferenceUsageController(ReferencesDbContext referencesDbContext, IReferenceUsageRepository referenceUsageRepository)
        {
            _referenceUsageRepository = referenceUsageRepository;
        }

        [HttpGet]
        public async Task<List<ReferenceUsage>> GetAll(int offset = 0, int limit = 10)
        {
            return await _referenceUsageRepository.GetAll(offset, limit);
        }
        [HttpGet]
        [Route("Reference/{id}")]
        public async Task<List<ReferenceUsage>> Get(Guid id)
        {
            return await _referenceUsageRepository.GetFromReferenceId(id);
        }

        [HttpGet]
        [Route("Count")]
        public async Task<ActionResult<int>> GetCount()
        {
            return await _referenceUsageRepository.CountAsync();
        }

        [Authorize("WriteAccess")]
        [HttpDelete("{id}")]
        [ProducesResponseType(404)]
        public Microsoft.AspNetCore.Mvc.ActionResult DeleteAllUsages(Guid id)
        {
            
            var entities = _referencesDbContext.ReferenceUsage.Where(x => x.ReferenceId == id).ToArray();
            if (entities.Length == 0) return NotFound();
            
            _referencesDbContext.ReferenceUsage.RemoveRange(entities);
            _referencesDbContext.SaveChanges();
            return Ok();

        }

        [Authorize("WriteAccess")]
        [HttpDelete("{id},{applicationId},{userId}")]
        public Microsoft.AspNetCore.Mvc.ActionResult DeleteUsage(Guid id, int applicationId, Guid userId)
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