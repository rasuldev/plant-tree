using System.Linq;
using System.Threading.Tasks;
using AuthTokenServer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlantTree.Data;
using PlantTree.Data.Entities;

namespace PlantTree.Controllers
{
    [Authorize(Roles = UserRoles.Admin)]
    public class ProjectUsersController : Controller
    {
        private readonly AppDbContext _context;

        public ProjectUsersController(AppDbContext context)
        {
            _context = context;    
        }

        // GET: ProjectUsers
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.ProjectUsers.Include(p => p.Project).Include(p => p.User);
            return View(await appDbContext.ToListAsync());
        }

        // GET: ProjectUsers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projectUser = await _context.ProjectUsers.SingleOrDefaultAsync(m => m.Id == id);
            if (projectUser == null)
            {
                return NotFound();
            }

            return View(projectUser);
        }

        // GET: ProjectUsers/Create
        public IActionResult Create()
        {
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName");
            return View();
        }

        // POST: ProjectUsers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProjectId,UserId")] ProjectUser projectUser)
        {
            if (ModelState.IsValid)
            {
                _context.Add(projectUser);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", projectUser.ProjectId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName", projectUser.UserId);
            return View(projectUser);
        }

        // GET: ProjectUsers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projectUser = await _context.ProjectUsers.SingleOrDefaultAsync(m => m.Id == id);
            if (projectUser == null)
            {
                return NotFound();
            }
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", projectUser.ProjectId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName", projectUser.UserId);
            return View(projectUser);
        }

        // POST: ProjectUsers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProjectId,UserId")] ProjectUser projectUser)
        {
            if (id != projectUser.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(projectUser);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectUserExists(projectUser.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", projectUser.ProjectId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName", projectUser.UserId);
            return View(projectUser);
        }

        // GET: ProjectUsers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projectUser = await _context.ProjectUsers.SingleOrDefaultAsync(m => m.Id == id);
            if (projectUser == null)
            {
                return NotFound();
            }

            return View(projectUser);
        }

        // POST: ProjectUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var projectUser = await _context.ProjectUsers.SingleOrDefaultAsync(m => m.Id == id);
            _context.ProjectUsers.Remove(projectUser);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool ProjectUserExists(int id)
        {
            return _context.ProjectUsers.Any(e => e.Id == id);
        }
    }
}
