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
    /// <summary>
    /// Get all usages
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<List<ReferenceUsage>>> GetAll(int offset = 0, int limit = 10)
    {
        var usages = await referenceUsageRepository.GetAll(offset, limit);
        return Ok(usages);
    }

    /// <summary>
    /// Get usages for a reference
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("Reference/{id:guid}")]
    public async Task<ActionResult<List<ReferenceUsage>>> Get(Guid id)
    {
        var usages = await referenceUsageRepository.GetFromReferenceId(id);
        return Ok(usages);
    }

    /// <summary>
    /// Get the number of usages in dB
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("Count")]
    public async Task<ActionResult<int>> GetCount()
    {
        var count = await referenceUsageRepository.CountAsync();
        return Ok(count);
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

    /// <summary>
    /// Delete Usage
    /// </summary>
    /// <param name="id"></param>
    /// <param name="applicationId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    [Authorize("WriteAccess")]
    [HttpDelete("{id:guid},{applicationId:int},{userId:guid}")]
    public ActionResult DeleteUsage(Guid id, int applicationId, Guid userId)
    {
        try
        {
            referenceUsageRepository.DeleteUsage(id, applicationId, userId);
        }
        catch (NotFoundException e)
        {
            return NotFound(e);
        }

        return Ok();
    }

    /// <summary>
    /// Add a new usage
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [Authorize("WriteAccess")]
    [HttpPost]
    public async Task<ActionResult<ReferenceUsage>> Post([FromBody] ReferenceUsage value)
    {
        if (value == null)
        {
            return BadRequest("No data posted");
        }

        await referenceUsageRepository.Add(value);

        return Ok(value);
    }

    /// <summary>
    /// Add a list of usages
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [Authorize("WriteAccess")]
    [HttpPost("bulk")]
    public async Task<ActionResult<bool>> Post(ReferenceUsage[] value)
    {
        if (value == null)
        {
            return BadRequest("No data posted");
        }

        var result = await referenceUsageRepository.AddRange(value);
        return Ok(result);
    }
}
