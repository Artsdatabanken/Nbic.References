using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Nbic.References.Controllers;

using Microsoft.AspNetCore.Authorization;

[Route("api/[controller]")]
[ApiController]
[SwaggerTag("Read, add or delete ReferencesUsages")]
public class ReferenceUsageController(IReferenceUsageRepository referenceUsageRepository) : ControllerBase
{
    [HttpGet]
    public async Task<List<ReferenceUsage>> GetAll(int offset = 0, int limit = 10)
    {
        return await referenceUsageRepository.GetAll(offset, limit);
    }
    [HttpGet]
    [Route("Reference/{id:guid}")]
    public async Task<List<ReferenceUsage>> Get(Guid id)
    {
        return await referenceUsageRepository.GetFromReferenceId(id);
    }

    [HttpGet]
    [Route("Count")]
    public async Task<ActionResult<int>> GetCount()
    {
        return await referenceUsageRepository.CountAsync();
    }

    /// <summary>
    /// Delete Usage for Reference
    /// </summary>
    /// <param name="id">Reference Id</param>
    /// <returns></returns>
    [Authorize("WriteAccess")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(404)]
    public ActionResult DeleteAllUsages(Guid id)
    {
        try
        {
            referenceUsageRepository.DeleteForReference(id);
        }
        catch (NotFoundException e)
        {
            return NotFound(e);
        }
            
        return Ok();
    }

    [Authorize("WriteAccess")]
    [HttpDelete("{id:guid},{applicationId:int},{userId:guid}")]
    public ActionResult DeleteUsage(Guid id, int applicationId, Guid userId)
    {
        try
        {
            referenceUsageRepository.DeleteUsage(id, applicationId,userId);
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

        await referenceUsageRepository.Add(value);
            
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

        return await referenceUsageRepository.AddRange(value);
    }
}