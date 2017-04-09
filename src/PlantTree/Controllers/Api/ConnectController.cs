using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NuGet.Common;
using PlantTree.Infrastructure.Common;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace PlantTree.Controllers.Api
{
    // This is only for swagger. Real processing of token request takes place in middleware (see AuthTokenServer)
    //[Produces("application/json")]
    [Route("api/connect")]
    public class ConnectController : Controller
    {
        // POST api/values
        [HttpPost("token")]
        //[ProducesResponseType(typeof(AuthToken),200)]
        public AuthToken PostToken([FromForm]string username, [FromForm]string password, [FromForm]string grant_type, [FromForm]string scope)
        {
            return new AuthToken();
        }

        // Response type for swagger
        public class AuthToken
        {
            public string scope { get; set; }
            public string token_type { get; set; }
            public string access_token { get; set; }
            public string expires_in { get; set; }
            public string refresh_token { get; set; }
            public string id_token { get; set; }
        }
    }
}
