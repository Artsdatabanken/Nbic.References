using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nbic.References.Core.Exceptions;
using Nbic.References.Core.Interfaces.Repositories;
using Nbic.References.Core.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace Nbic.References.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Create, read, update and delete References")]
    public class ReferencesController : ControllerBase
    {
        private readonly IReferencesRepository _referencesRepository;

        public ReferencesController(IReferencesRepository referencesRepository)
        {
            _referencesRepository = referencesRepository;
        }

        [HttpGet]
        public async Task<List<Reference>> GetAll(int offset = 0, int limit = 10, string search = null)
        {
            return await _referencesRepository.Search(search, offset, limit);
        }

        [HttpGet]
        [Route("Count")]
        public async Task<ActionResult<int>> GetCount()
        {
            return await _referencesRepository.GetReferencesCountAsync(); // _referencesDbContext.Reference.CountAsync().ConfigureAwait(false);
        }

        [HttpGet]
        [Authorize("WriteAccess")]
        [Route("Reindex")]
        public ActionResult<bool> DoReindex()
        {
            _referencesRepository.ReIndex();
            return true;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Reference>> Get(Guid id)
        {
            var reference = await _referencesRepository.Get(id);
            if (reference == null) return NotFound();
            
            return reference;
        }

        [Authorize("WriteAccess")]
        [HttpPost]
        public async Task<ActionResult<Reference>> Post([FromBody] Reference value)
        {
            if (value == null)
            {
                return BadRequest("No data posted");
            }

            Reference newReference;
            try
            {
                newReference = await _referencesRepository.Add(value);
            }
            catch (BadRequestException e)
            {
                return BadRequest(e);
            }

            return newReference;
        }
        
        [Authorize("WriteAccess")]
        [HttpPost]
        [Route("Bulk")]
        public async Task<ActionResult> PostMany([FromBody] Reference[] values)
        {
            if (values == null || values.Length == 0)
            {
                return BadRequest("No data posted");
            }
            try
            {
                await _referencesRepository.AddRange(values);
            }
            catch (BadRequestException e)
            {
                return BadRequest(e);
            }
            
            return Ok();
        }

        [Authorize("WriteAccess")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(Guid id, [FromBody] Reference value)
        {
            if (value == null)
            {
                return BadRequest("No reference to put");
            }

            try
            {
                await _referencesRepository.Update(value);
            }
            catch (NotFoundException e)
            {
                return NotFound(e);
            }
           
            return Ok();
        }

        [Authorize("WriteAccess")]
        [HttpDelete("{id}")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        public ActionResult Delete(Guid id)
        {
            try
            {
                _referencesRepository.Delete(id);
            }
            catch (NotFoundException e)
            {
                return NotFound(e);
            }

            return Ok();

        }
    }
}
