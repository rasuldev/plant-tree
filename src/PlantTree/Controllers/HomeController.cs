using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthTokenServer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantTree.Data;
using PlantTree.Infrastructure.Common;

namespace PlantTree.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        [Authorize(Roles = UserRoles.Admin)]
        public IActionResult Seed([FromServices] AppDbContext context)
        {
            DbSeeder.Seed(context);
            return Ok("done");
        }
    }
}
