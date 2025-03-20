using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Nbic.References.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Check status and authentication")]
    public class PingController : ControllerBase
    {
        [HttpGet]
        [Route("Ping")]
        public string Ping()
        {
            return "Ok!";
        }

        [HttpGet]
        [Authorize("WriteAccess")]
        [Route("AuthorisedPing")]
        public string AuthorisedPing()
        {
            return "Ok!";
        }
    }
}
