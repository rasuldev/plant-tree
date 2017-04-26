using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthTokenServer.Common;
using Common.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantTree.Data;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace PlantTree.Controllers.Api
{
    [Area("api")]
    [Route("api/[controller]")]
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly AppDbContext _context;

        public TransactionsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/values
        [HttpGet]
        public async Task<IActionResult> Get(int page = 1, int pagesize = 10)
        {
            var userId = SecurityRoutines.GetUserId(HttpContext);
            if (userId == null)
                return new ApiErrorResult("User id is null");
            var transactions = await _context.Transactions.Where(t => t.UserId == userId)
                                        .Skip((page - 1) * pagesize).Take(pagesize).AsNoTracking().ToListAsync();
            return Ok(transactions);
        }

    }
}
