using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _db;

        public CategoriesController(RaceDayDbContext db)
        {
            _db = db;
        }

        // =========================================================
        // PUBLIC - Both roles can view categories
        // =========================================================

        /// <summary>
        /// GET /api/events/{eventId}/categories
        /// Lists all categories for a specific event. Accessible to all logged-in users.
        /// </summary>
        [HttpGet("events/{eventId:int}/categories")]
        public async Task<IActionResult> GetForEvent(int eventId)
        {
            var exists = await _db.Events.AnyAsync(e => e.EventId == eventId);
            if (!exists)
                return NotFound(new { message = "Event not found." });

            var categories = await _db.Categories
                .Where(c => c.EventId == eventId)
                .OrderBy(c => c.EntryFee)
                .Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    EventId = c.EventId,
                    Name = c.Name,
                    Description = c.Description,
                    EntryFee = c.EntryFee
                })
                .ToListAsync();

            return Ok(categories);
        }

        // =========================================================
        // ORGANISER ONLY - manage categories for own events
        // =========================================================

        /// <summary>
        /// POST /api/events/{eventId}/categories
        /// Creates a category. Organiser must own the parent event.
        /// </summary>
        [HttpPost("events/{eventId:int}/categories")]
        [RoleAuthorizeFilter("Organiser")]
        public async Task<IActionResult> Create(int eventId, [FromBody] CreateCategoryRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = HttpContext.Session.GetInt32("UserId");
            var organiser = await _db.OrganiserProfiles
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (organiser == null)
                return Unauthorized();

            // Ownership check: only allow if organiser owns the event
            var evt = await _db.Events
                .FirstOrDefaultAsync(e => e.EventId == eventId
                                       && e.OrganiserId == organiser.OrganiserId);

            if (evt == null)
                return NotFound(new { message = "Event not found or you do not own it." });

            var category = new Category
            {
                EventId = eventId,
                Name = req.Name,
                Description = req.Description,
                EntryFee = req.EntryFee
            };

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Category created successfully.",
                categoryId = category.CategoryId
            });
        }

        /// <summary>
        /// PUT /api/categories/{id}
        /// Updates a category. Organiser must own the parent event.
        /// </summary>
        [HttpPut("categories/{id:int}")]
        [RoleAuthorizeFilter("Organiser")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateCategoryRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = HttpContext.Session.GetInt32("UserId");
            var organiser = await _db.OrganiserProfiles
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (organiser == null)
                return Unauthorized();

            // Ownership check via navigation to Event
            var category = await _db.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id
                                       && c.Event!.OrganiserId == organiser.OrganiserId);

            if (category == null)
                return NotFound(new { message = "Category not found or you do not own it." });

            category.Name = req.Name;
            category.Description = req.Description;
            category.EntryFee = req.EntryFee;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Category updated successfully." });
        }

        /// <summary>
        /// DELETE /api/categories/{id}
        /// Deletes a category. Organiser must own the parent event.
        /// </summary>
        [HttpDelete("categories/{id:int}")]
        [RoleAuthorizeFilter("Organiser")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var organiser = await _db.OrganiserProfiles
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (organiser == null)
                return Unauthorized();

            var category = await _db.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id
                                       && c.Event!.OrganiserId == organiser.OrganiserId);

            if (category == null)
                return NotFound(new { message = "Category not found or you do not own it." });

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}