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

        public ReferenceUsageController(IReferenceUsageRepository referenceUsageRepository)
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

        /// <summary>
        /// Delete Usage for Reference
        /// </summary>
        /// <param name="id">Reference Id</param>
        /// <returns></returns>
        [Authorize("WriteAccess")]
        [HttpDelete("{id}")]
        [ProducesResponseType(404)]
        public Microsoft.AspNetCore.Mvc.ActionResult DeleteAllUsages(Guid id)
        {
            try
            {
                _referenceUsageRepository.DeleteForReference(id);
            }
            catch (NotFoundException e)
            {
                return NotFound(e);
            }
            
            return Ok();
        }

        [Authorize("WriteAccess")]
        [HttpDelete("{id},{applicationId},{userId}")]
        public Microsoft.AspNetCore.Mvc.ActionResult DeleteUsage(Guid id, int applicationId, Guid userId)
        {
            try
            {
                _referenceUsageRepository.DeleteUsage(id, applicationId,userId);
            }
            catch (NotFoundException e)
            {
                return NotFound(e);
            }
            
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

            await _referenceUsageRepository.Add(value);
            
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

            return await _referenceUsageRepository.AddRange(value);
        }
    }
}