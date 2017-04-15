using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Errors;
using Common.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlantTree.Data;
using PlantTree.Data.Entities;
using PlantTree.Infrastructure.Common;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace PlantTree.Controllers.Api
{
    [Route("api/news")]
    public class NewsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly Repository _repository;
        private readonly ILogger<ProjectsController> _logger;

        public NewsController(AppDbContext context, Repository repository, ILogger<ProjectsController> logger)
        {
            _context = context;
            _repository = repository;
            _logger = logger;
        }

        [HttpGet]
        [HttpGet("/page/{page:int}")]
        [HttpGet("/page/{page:int}/pagesize/{pagesize:int}")]
        [HttpGet("/project/{projectId:int}")]
        [HttpGet("/project/{projectId:int}/page/{page:int}")]
        [HttpGet("/project/{projectId:int}/page/{page:int}/pagesize/{pagesize:int}")]
        [ProducesResponseType(typeof(List<News>), 200)]
        [ProducesResponseType(typeof(List<ApiError>), 400)]
        public async Task<IActionResult> GetNews(int? projectId = null, int page = 1, int pagesize = 5)
        {
            if (page <= 0)
                return new ApiErrorResult($"Parameter {nameof(page)} should be greater than 0");
            if (pagesize <= 0)
                return new ApiErrorResult($"Parameter {nameof(pagesize)} should be greater than 0");

            var news = await _repository.GetNews(projectId, page, pagesize);
            return Ok(news);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(News), 200)]
        [ProducesResponseType(typeof(List<ApiError>), 400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetNewsItem(int id)
        {
            var newsItem = await _context.News.Include(n => n.Photo).SingleOrDefaultAsync(n => n.Id == id);
            if (newsItem == null)
                return NotFound();
            return Ok(newsItem);
        }
    }
}
