using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PlantTree.Data.Misc;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace PlantTree.Controllers.Api
{
    [Area("api")]
    [Route("api/about")]
    public class AboutController : Controller
    {
        // GET: api/values
        [HttpGet]
        [Produces("text/html")]
        public string Get([FromServices] StaticContentContext context)
        {
            return context.LoadAbout();
        }

        
    }
}
