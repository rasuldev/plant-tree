using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthTokenServer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlantTree.Data;
using PlantTree.Data.Entities;

namespace PlantTree.Controllers.Api
{
    [Produces("application/json")]
    [Route("api/Images")]
    [Authorize(Roles = UserRoles.Admin)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ImagesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ImagesController> _logger;

        public ImagesController(AppDbContext context, ILogger<ImagesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Images
        [HttpGet]
        public IEnumerable<Image> GetImages()
        {
            return _context.Images;
        }

        // GET: api/Images/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetImage([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Image image = await _context.Images.SingleOrDefaultAsync(m => m.Id == id);

            if (image == null)
            {
                return NotFound();
            }

            return Ok(image);
        }

        // PUT: api/Images/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutImage([FromRoute] int id, [FromBody] Image image)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != image.Id)
            {
                return BadRequest();
            }

            _context.Entry(image).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ImageExists(id))
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

        // POST: api/Images
        [HttpPost]
        public async Task<IActionResult> PostImage([FromBody] Image image)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Images.Add(image);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ImageExists(image.Id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetImage", new { id = image.Id }, image);
        }

        // DELETE: api/Images/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Image image = await _context.Images.SingleOrDefaultAsync(m => m.Id == id);
            if (image == null)
            {
                return NotFound();
            }

            _context.Images.Remove(image);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }


            return Ok();
        }

        private bool ImageExists(int id)
        {
            return _context.Images.Any(e => e.Id == id);
        }
    }
}