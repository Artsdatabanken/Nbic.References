using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Nbic.References.Controllers;

[Route("api/[controller]")]
[ApiController]
[SwaggerTag("Create, read, update and delete References")]
public class ReferencesController(IReferencesRepository referencesRepository) : ControllerBase
{
    /// <summary>
    /// Get all references
    /// </summary>
    /// <param name="offset">Start at reference number</param>
    /// <param name="limit">Number of references</param>
    /// <param name="search">Free text search</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<List<Reference>>> GetAll(int offset = 0, int limit = 10, string search = null)
    {
        return Ok(await referencesRepository.Search(search, offset, limit));
    }

    /// <summary>
    /// Get the number of references in dB
    /// </summary>
    /// <returns>Number of references</returns>
    [HttpGet]
    [Route("Count")]
    public async Task<ActionResult<int>> GetCount()
    {
        var count = await referencesRepository.CountAsync();
        return Ok(count);
    }


    /// <summary>
    /// Administrative endpoint to force reindex of references
    /// </summary>
    /// <returns>Ok if done</returns>
    [HttpGet]
    [Authorize("WriteAccess")]
    [Route("Reindex")]
    public ActionResult<bool> DoReindex()
    {
        referencesRepository.ReIndex();
        return Ok(true);
    }


    /// <summary>
    /// Get Reference by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Reference>> Get(Guid id)
    {
        var reference = await referencesRepository.Get(id);
        if (reference == null) return NotFound();

        return Ok(reference);
    }


    /// <summary>
    /// Add a new reference
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
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
            newReference = await referencesRepository.Add(value);
        }
        catch (BadRequestException e)
        {
            return BadRequest(e.Message);
        }

        return Ok(newReference);
    }


    /// <summary>
    /// Add many references
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
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
            await referencesRepository.AddRange(values);
        }
        catch (BadRequestException e)
        {
            return BadRequest(e.Message);
        }

        return Ok();
    }


    /// <summary>
    /// Update a reference by Id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    [Authorize("WriteAccess")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Put(Guid id, [FromBody] Reference value)
    {
        if (value == null)
        {
            return BadRequest("No reference to put");
        }

        if (value.Id == Guid.Empty)
        {
            value.Id = id;
        }

        if (value.Id != id)
        {
            return BadRequest("Id on reference set and different...");
        }

        try
        {
            await referencesRepository.Update(value);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }

        return Ok();
    }


    /// <summary>
    /// Delete a reference by Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize("WriteAccess")]
    [HttpDelete("{id:guid}")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
    public ActionResult Delete(Guid id)
    {
        try
        {
            referencesRepository.Delete(id);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }

        return Ok();
    }
}