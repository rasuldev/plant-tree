using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlantTree.Data;
using PlantTree.Data.Entities;
using PlantTree.Infrastructure.Common;

namespace PlantTree.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class ProjectsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ImageFactory _imageFactory;

        public ProjectsController(AppDbContext context, ImageFactory imageFactory)
        {
            _context = context;
            _imageFactory = imageFactory;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Projects.Include(p => p.MainImage);            
            return View(await appDbContext.ToListAsync());
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.SingleOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        public IActionResult Create()
        {
            CreateOrEditInit();
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Status,Description,Goal,ImageId,Name,Reached,ShortDescription,Tag")] Project project, IFormFile mainImage, ICollection<IFormFile> otherImages)
        {
            if (ModelState.IsValid)
            {
                await ProcessImages(project, mainImage, otherImages);

                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            CreateOrEditInit();

            return View(project);
        }

        private async Task ProcessImages(Project project, IFormFile mainImage, ICollection<IFormFile> otherImages)
        {
            if (mainImage != null)
            {
                var image = await _imageFactory.CreateProjectImage(mainImage);
                project.MainImage = image;
            }
            if (otherImages != null && otherImages.Count > 0)
            {
                var images = new List<Image>();
                foreach (var otherImage in otherImages)
                {
                    var image = await _imageFactory.CreateProjectImage(otherImage);
                    images.Add(image);
                }
                project.OtherImages = images;
            }
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var project = await _context.Projects
                .Include(p => p.MainImage)
                .Include(p => p.OtherImages)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }
            CreateOrEditInit();
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,Goal,ImageId,Status,Name,Reached,ShortDescription,Tag")] Project project, IFormFile mainImage, ICollection<IFormFile> otherImages)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await ProcessImages(project, mainImage, otherImages);
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
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
            CreateOrEditInit();
            return View(project);
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.SingleOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.SingleOrDefaultAsync(m => m.Id == id);
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }

        private void CreateOrEditInit()
        {
            var statusItems = Enum.GetValues(typeof(ProjectStatus)).Cast<ProjectStatus>().Select(x => new { code = x, value = x.ToString() });
            ViewData["StatusList"] = new SelectList(statusItems, "code", "value", ProjectStatus.InProgress);
        }
    }
}
