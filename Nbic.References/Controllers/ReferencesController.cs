using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public ActionResult<IEnumerable<RfReference>> GetAll(int offset = 0, int limit = 10)
        {
            return _referencesDbContext.RfReference.OrderBy(x => x.PkReferenceId)
                .Skip(offset).Take(limit).ToArray(); // new RfReference[] { new RfReference(){ApplicationId = 1}, new RfReference() { ApplicationId = 2 } };
        }

        [HttpGet("{id}")]
        public ActionResult<RfReference> Get(Guid id)
        {
            var reference = _referencesDbContext.RfReference.FirstOrDefault(x=>x.PkReferenceId == id);
            if (reference == null)
            {
                return NotFound();
            }

            return reference;
        }

        [Authorize]
        [HttpPost]
        public ActionResult<RfReference> Post([FromBody] RfReference value)
        {
            if (value == null)
            {
                return BadRequest("No data posted");
            }

            if (value.PkReferenceId == Guid.Empty)
            {
                value.PkReferenceId = Guid.NewGuid();
            }

            _referencesDbContext.RfReference.Add(value);
            try
            {
                var recordsSaved = _referencesDbContext.SaveChanges();
            }
            catch (SqlException e)
            {
                if (e.Message.Contains("Violation of PRIMARY KEY constraint"))
                {
                    return BadRequest("Violation of PRIMARY KEY constraint. Key exists!");
                }
            }
        
            return value;
        }
        [Authorize]
        [HttpPost]
        [Route("bulk")]
        public ActionResult PostMany([FromBody] RfReference[] values)
        {
            //if (value == null)
            //{
            //    return BadRequest("No data posted");
            //}

            //if (value.PkReferenceId == Guid.Empty)
            //{
            //    value.PkReferenceId = Guid.NewGuid();
            //}

            //_referencesDbContext.RfReference.Add(value);
            //try
            //{
            //    var recordsSaved = _referencesDbContext.SaveChanges();
            //}
            //catch (SqlException e)
            //{
            //    if (e.Message.Contains("Violation of PRIMARY KEY constraint"))
            //    {
            //        return BadRequest("Violation of PRIMARY KEY constraint. Key exists!");
            //    }
            //}
            return Ok();
//            return value;
        }

        [Authorize]
        [HttpPut("{id}")]
        public ActionResult Put(Guid id, [FromBody] RfReference value)
        {
            var r = _referencesDbContext.RfReference.FirstOrDefault(x => x.PkReferenceId == id);
            if (r == null)
            {
                return NotFound();
            }
            // transfer values
            // these first three should never be changed
            //if (r.PkReferenceId != value.PkReferenceId) r.PkReferenceId = value.PkReferenceId;
            //if (r.ApplicationId != value.ApplicationId) r.ApplicationId = value.ApplicationId;
            //if (r.FkUserId != value.FkUserId) r.FkUserId = value.FkUserId;
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

            r.EditDate = DateTime.Now;
            
            // todo usages

            _referencesDbContext.SaveChanges();
            return Ok();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public ActionResult Delete(Guid id)
        {
            var item = _referencesDbContext.RfReference.FirstOrDefault(x => x.PkReferenceId == id);
            if (item == null) return NotFound();
            _referencesDbContext.RfReference.Remove(item);
            _referencesDbContext.SaveChanges();
            return Ok();

        }
    }
}
