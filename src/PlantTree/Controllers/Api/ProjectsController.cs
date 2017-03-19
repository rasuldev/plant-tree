using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthTokenServer.Common;
using Common.Results;
using Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PlantTree.Data;
using PlantTree.Data.Entities;
using PlantTree.Infrastructure.Common;

namespace PlantTree.Controllers.Api
{
    [Produces("application/json")]
    [Route("api/projects")]
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly Repository _repository;
        private ILogger _logger;

        public ProjectsController(AppDbContext context, Repository repository, ILogger<ProjectsController> logger)
        {
            _context = context;
            _repository = repository;
            _logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            GlobalConf.Host = $"{Request.Scheme}://{Request.Host}";
            base.OnActionExecuting(context);
        }

        // GET: api/Projects
        // GET: api/Projects/page/3
        // GET: api/Projects/page/3/pagesize/30
        [HttpGet]
        [HttpGet("page/{page:int}")]
        [HttpGet("page/{page:int}/pagesize/{pagesize:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProjects(int page = 1, int pagesize = 20)
        {
            var userId = SecurityRoutines.GetUserId(HttpContext);
            // If for authenticated request userId is null (it means that access_token is wrong) we return 401.
            if (SecurityRoutines.IsRequestAuthorized(HttpContext) && userId == null)
                return Unauthorized();

            if (page <= 0)
                return new ApiErrorResult($"Parameter {nameof(page)} should be greater than 0");
            if (pagesize <= 0)
                return new ApiErrorResult($"Parameter {nameof(pagesize)} should be greater than 0");

            var projects = await _repository.GetProjects(page, pagesize);

            if (userId == null)
                return Ok(projects);

            var userProjects = new HashSet<int>(await _context.ProjectUsers.Where(pu => pu.UserId == userId).Select(pu => pu.ProjectId).ToListAsync());
            foreach (var project in projects)
            {
                if (userProjects.Contains(project.Id))
                    project.IsLiked = true;
            }
            return Ok(projects);
        }

        // GET: api/Projects/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProject([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Project project = await _context.Projects
                .Include(p => p.MainImage)
                .Include(p => p.OtherImages)
                .SingleOrDefaultAsync(m => m.Id == id);

            if (project == null)
            {
                return NotFound();
            }
            return Ok(project);
        }

        // GET: api/Projects/user


        // GET: api/Projects/user
        // GET: api/Projects/user/page/3
        // GET: api/Projects/user/page/3/pagesize/30
        [HttpGet("user")]
        [HttpGet("user/page/{page:int}")]
        [HttpGet("user/page/{page:int}/pagesize/{pagesize:int}")]
        public async Task<IEnumerable<Project>> GetUserProjects(int page = 1, int pagesize = 20)
        {
            //var username = User.Identity.Name;
            var userId = SecurityRoutines.GetUserId(HttpContext);
            //var user = await _context.AllUsers.SingleOrDefaultAsync(u => u.Login == login && u.Password == password);
            //if (user == null)
            //{
            //    return NotFound();
            //}

            var userProjectsId = _context.ProjectUsers
                .Where(pu => pu.UserId == userId)
                .OrderBy(pu => pu.ProjectId)
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .Select(pu => pu.ProjectId);

            var userProjects = await _context.Projects
                .Include(p => p.MainImage)
                .Include(p => p.OtherImages)
                .Include(p => p.ProjectUsers)
                .Where(p => userProjectsId.Contains(p.Id))
                .ToListAsync();

            foreach (var project in userProjects)
            {
                project.IsLiked = true;
            }
            return userProjects;
            //var userProjects = _context.Projects
            //    .Include(p => p.MainImage).Include(p => p.OtherImages)
            //    .Join(_context.ProjectUsers,
            //          project => project.Id, projectUser => projectUser.ProjectId,
            //          (project, projectUser) => new { project, userId = projectUser.UserId })
            //    .Where(p => p.userId == userId)
            //    .Select(p => p.project);
            //return await userProjects.ToListAsync();


            //List<Project> projects = userProjects.Select(pu =>
            //{
            //    //pu.Project.ProjectUsers = null;
            //    return pu.Project;
            //}).ToList();
            ////List<Project> projects = _context.Projects
            ////    .Include(p => p.MainImage).Include(p => p.OtherImages).ToList();

            ////return Content(projects.ToJson());
            //return projects;
        }

        // GET: api/Projects/5/like
        [HttpPut("{id}/like")]
        public async Task<IActionResult> MakeProjectFavourite([FromRoute] int id)
        {
            var userId = SecurityRoutines.GetUserId(HttpContext);
            if (_context.ProjectUsers.Any(pu => pu.ProjectId == id && pu.UserId == userId))
            {
                // Already added to favourites for this user
                return NoContent();
            }

            var linkEntity = new ProjectUser() { ProjectId = id, UserId = userId };
            _context.ProjectUsers.Add(linkEntity);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (!ProjectExists(id))
                {
                    return NotFound($"No project with id={id}");
                }
                else
                {
                    return new CodeWithContentResult("Db error", StatusCodes.Status409Conflict);
                }
            }
            return NoContent();
        }

        // GET: api/Projects/5/dislike
        [HttpPut("{id}/dislike")]
        public async Task<IActionResult> RemoveProjectFromFavourites([FromRoute] int id)
        {
            var userId = User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var link = await _context.ProjectUsers.SingleOrDefaultAsync(pu => pu.ProjectId == id && pu.UserId == userId);
            if (link != null)
            {
                _context.Remove(link);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {
                    return new CodeWithContentResult("Db error", StatusCodes.Status409Conflict);
                }
            }
            return NoContent();
        }

        // PUT: api/Projects/5
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPut("{id}")] 
        public async Task<IActionResult> PutProject([FromRoute] int id, [FromBody] Project project)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != project.Id)
            {
                return BadRequest();
            }

            _context.Entry(project).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Projects
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        public async Task<IActionResult> PostProject([FromBody] Project project)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Projects.Add(project);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ProjectExists(project.Id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetProject", new { id = project.Id }, project);
        }

        // DELETE: api/Projects/5
        [Authorize(Roles = UserRoles.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Project project = await _context.Projects.SingleOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return Ok(project);
        }

        [AllowAnonymous]
        [HttpGet("test")]
        public async Task<IActionResult> TestEmail([FromServices] IEmailSender emailSender)
        {
            await emailSender.SendEmailAsync("rasuldev@gmail.com", "Tooba.org", "Hello");
            return Ok();
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}