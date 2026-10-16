using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _db;

        public EventsController(RaceDayDbContext db)
        {
            _db = db;
        }

        // =========================================================
        // PUBLIC - Both roles can view events
        // =========================================================

        /// <summary>
        /// GET /api/events
        /// Lists all events. Accessible to any logged-in user.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var events = await _db.Events
                .OrderBy(e => e.EventDate)
                .Select(e => new EventDto
                {
                    EventId = e.EventId,
                    Name = e.Name,
                    Description = e.Description,
                    EventDate = e.EventDate,
                    Location = e.Location,
                    DistanceKm = e.DistanceKm,
                    RouteDetails = e.RouteDetails,
                    EventType = e.EventType,
                    MaxParticipants = e.MaxParticipants,
                    OrganiserId = e.OrganiserId,
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync();

            return Ok(events);
        }

        /// <summary>
        /// GET /api/events/{id}
        /// Retrieves a single event with its categories.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var evt = await _db.Events
                .Include(e => e.Categories)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (evt == null)
                return NotFound(new { message = "Event not found." });

            return Ok(new
            {
                evt.EventId,
                evt.Name,
                evt.Description,
                evt.EventDate,
                evt.Location,
                evt.DistanceKm,
                evt.RouteDetails,
                evt.EventType,
                evt.MaxParticipants,
                evt.OrganiserId,
                evt.CreatedAt,
                Categories = evt.Categories.Select(c => new
                {
                    c.CategoryId,
                    c.Name,
                    c.Description,
                    c.EntryFee
                })
            });
        }

        // =========================================================
        // ORGANISER ONLY - manage own events
        // =========================================================

        /// <summary>
        /// GET /api/events/mine
        /// Lists all events belonging to the logged-in Organiser.
        /// </summary>
        [HttpGet("mine")]
        [RoleAuthorizeFilter("Organiser")]
        public async Task<IActionResult> GetMine()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var organiser = await _db.OrganiserProfiles
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (organiser == null)
                return Unauthorized(new { message = "Organiser profile not found." });

            var events = await _db.Events
                .Where(e => e.OrganiserId == organiser.OrganiserId)
                .OrderByDescending(e => e.EventDate)
                .Select(e => new EventDto
                {
                    EventId = e.EventId,
                    Name = e.Name,
                    Description = e.Description,
                    EventDate = e.EventDate,
                    Location = e.Location,
                    DistanceKm = e.DistanceKm,
                    RouteDetails = e.RouteDetails,
                    EventType = e.EventType,
                    MaxParticipants = e.MaxParticipants,
                    OrganiserId = e.OrganiserId,
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync();

            return Ok(events);
        }

        /// <summary>
        /// POST /api/events
        /// Creates a new event. Organiser only.
        /// </summary>
        [HttpPost]
        [RoleAuthorizeFilter("Organiser")]
        public async Task<IActionResult> Create([FromBody] CreateEventRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = HttpContext.Session.GetInt32("UserId");
            var organiser = await _db.OrganiserProfiles
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (organiser == null)
                return Unauthorized(new { message = "Organiser profile not found." });

            var evt = new Event
            {
                OrganiserId = organiser.OrganiserId,
                Name = req.Name,
                Description = req.Description,
                EventDate = req.EventDate,
                Location = req.Location,
                DistanceKm = req.DistanceKm,
                RouteDetails = req.RouteDetails,
                EventType = req.EventType,
                MaxParticipants = req.MaxParticipants
            };

            _db.Events.Add(evt);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = evt.EventId }, new
            {
                message = "Event created successfully.",
                eventId = evt.EventId
            });
        }

        /// <summary>
        /// PUT /api/events/{id}
        /// Updates an event the logged-in Organiser owns.
        /// </summary>
        [HttpPut("{id:int}")]
        [RoleAuthorizeFilter("Organiser")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEventRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = HttpContext.Session.GetInt32("UserId");
            var organiser = await _db.OrganiserProfiles
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (organiser == null)
                return Unauthorized();

            // Ownership check: only find events owned by this organiser
            var evt = await _db.Events
                .FirstOrDefaultAsync(e => e.EventId == id
                                       && e.OrganiserId == organiser.OrganiserId);

            if (evt == null)
                return NotFound(new { message = "Event not found or you do not own it." });

            evt.Name = req.Name;
            evt.Description = req.Description;
            evt.EventDate = req.EventDate;
            evt.Location = req.Location;
            evt.DistanceKm = req.DistanceKm;
            evt.RouteDetails = req.RouteDetails;
            evt.EventType = req.EventType;
            evt.MaxParticipants = req.MaxParticipants;
            evt.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Event updated successfully." });
        }

        /// <summary>
        /// DELETE /api/events/{id}
        /// Deletes an event the logged-in Organiser owns.
        /// </summary>
        [HttpDelete("{id:int}")]
        [RoleAuthorizeFilter("Organiser")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var organiser = await _db.OrganiserProfiles
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (organiser == null)
                return Unauthorized();

            var evt = await _db.Events
                .FirstOrDefaultAsync(e => e.EventId == id
                                       && e.OrganiserId == organiser.OrganiserId);

            if (evt == null)
                return NotFound(new { message = "Event not found or you do not own it." });

            _db.Events.Remove(evt);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}