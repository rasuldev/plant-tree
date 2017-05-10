using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
        public AuthToken RequestToken([FromForm]string username, [FromForm]string password, [FromForm]string grant_type, [FromForm]string scope)
        {
            return new AuthToken();
        }

        // We add some fake methods in order to let swagger generate them
        // Actually we want not to change api path and let it equal to api/connect/token
        // But in this case swagger raises exception. So we use workaround: add to path #tag
        [HttpPost("token#social")]
        public AuthToken RequestTokenUsingSocial([FromForm]string identity_provider, [FromForm]string access_token, [FromForm]string grant_type, [FromForm]string scope)
        {
            return new AuthToken();
        }

        [HttpPost("token#refresh")]
        public AuthToken RefreshToken([FromForm]string refresh_token, [FromForm]string grant_type, [FromForm]string scope)
        {
            return new AuthToken();
        }

        // Response type for swagger
        public class AuthToken
        {
            public string scope { get; set; }
            public string token_type { get; set; }
            public string access_token { get; set; }
            public int expires_in { get; set; }
            public string refresh_token { get; set; }
            public string id_token { get; set; }
        }
    }
}
