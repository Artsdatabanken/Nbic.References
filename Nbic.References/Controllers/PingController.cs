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
        /// <summary>
        /// A simple unauthenticated ping endpoint
        /// </summary>
        /// <returns>Ok!</returns>
        [HttpGet]
        [Route("Ping")]
        public string Ping()
        {
            return "Ok!";
        }

        /// <summary>
        /// A simple authenticated ping endpoint verifying that the user has write access
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize("WriteAccess")]
        [Route("AuthorizedPing")]
        public string AuthorizedPing()
        {
            return "Ok!";
        }
    }
}
