using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Nbic.Reference.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReferencesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [Authorize]
        [HttpPost]
        public string Post([FromBody] string value)
        {
            return value;
        }

        // PUT api/values/5
        [Authorize]
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [Authorize]
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
