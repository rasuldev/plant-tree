using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthTokenServer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantTree.Data;
using PlantTree.Data.Entities;
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
            //var random = new Random();
            //var transactions = context.Transactions.Where(t => t.Status == TransactionStatus.Success && t.FinishedDate == null).ToList();
            //transactions.ForEach(t => t.FinishedDate = t.CreationDate + TimeSpan.FromDays(random.NextDouble() * 30));
            //context.SaveChanges();
            return Ok("done");
        }
    }
}
