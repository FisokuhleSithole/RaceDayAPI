using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/enrolments")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _db;

        public EnrolmentsController(RaceDayDbContext db)
        {
            _db = db;
        }

        // =========================================================
        // PARTICIPANT ONLY - Enrol in an event
        // =========================================================

        /// <summary>
        /// POST /api/enrolments
        /// Participant enters an event by selecting a category.
        /// Prevents duplicate enrolments.
        /// </summary>
        [HttpPost]
        [RoleAuthorizeFilter("Participant")]
        public async Task<IActionResult> Create([FromBody] CreateEnrolmentRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = HttpContext.Session.GetInt32("UserId");
            var participant = await _db.ParticipantProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (participant == null)
                return Unauthorized(new { message = "Participant profile not found." });

            // Verify category exists
            var category = await _db.Categories.FindAsync(req.CategoryId);
            if (category == null)
                return NotFound(new { message = "Category not found." });

            // Prevent duplicate enrolment
            var duplicate = await _db.Enrolments.AnyAsync(e =>
                e.ParticipantId == participant.ParticipantId
                && e.CategoryId == req.CategoryId);

            if (duplicate)
                return Conflict(new { message = "You are already enrolled in this category." });

            var enrolment = new Enrolment
            {
                ParticipantId = participant.ParticipantId,
                CategoryId = req.CategoryId,
                Status = "Pending"
            };

            _db.Enrolments.Add(enrolment);
            await _db.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Enrolment created successfully.",
                enrolmentId = enrolment.EnrolmentId
            });
        }

        /// <summary>
        /// GET /api/enrolments/mine
        /// Participant views their own enrolments.
        /// </summary>
        [HttpGet("mine")]
        [RoleAuthorizeFilter("Participant")]
        public async Task<IActionResult> GetMine()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var participant = await _db.ParticipantProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (participant == null)
                return Unauthorized();

            var list = await _db.Enrolments
                .Where(e => e.ParticipantId == participant.ParticipantId)
                .Include(e => e.Category).ThenInclude(c => c!.Event)
                .Include(e => e.Result)
                .OrderByDescending(e => e.EnrolmentDate)
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.Status,
                    e.EnrolmentDate,
                    Event = new
                    {
                        e.Category!.Event!.EventId,
                        e.Category.Event.Name,
                        e.Category.Event.EventDate,
                        e.Category.Event.Location,
                        e.Category.Event.DistanceKm
                    },
                    Category = new
                    {
                        e.Category!.CategoryId,
                        e.Category.Name,
                        e.Category.EntryFee
                    },
                    Result = e.Result == null ? null : new
                    {
                        e.Result.FinishTime,
                        e.Result.Position
                    }
                })
                .ToListAsync();

            return Ok(list);
        }

        // =========================================================
        // ORGANISER ONLY - Manage enrolments
        // =========================================================

        /// <summary>
        /// GET /api/enrolments/event/{eventId}
        /// Organiser views enrolments for one of their own events.
        /// </summary>
        [HttpGet("event/{eventId:int}")]
        [RoleAuthorizeFilter("Organiser")]
        public async Task<IActionResult> GetForEvent(int eventId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var organiser = await _db.OrganiserProfiles
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (organiser == null)
                return Unauthorized();

            // Ownership check
            var owns = await _db.Events
                .AnyAsync(e => e.EventId == eventId
                            && e.OrganiserId == organiser.OrganiserId);

            if (!owns)
                return NotFound(new { message = "Event not found or you do not own it." });

            var list = await _db.Enrolments
                .Where(e => e.Category!.EventId == eventId)
                .Include(e => e.Participant).ThenInclude(p => p!.User)
                .Include(e => e.Category)
                .Include(e => e.Result)
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.Status,
                    e.EnrolmentDate,
                    Participant = new
                    {
                        e.Participant!.ParticipantId,
                        Name = e.Participant.User!.FirstName + " " + e.Participant.User.LastName,
                        Email = e.Participant.User.Email,
                        Phone = e.Participant.PhoneNumber
                    },
                    Category = new { e.Category!.CategoryId, e.Category.Name },
                    Result = e.Result == null ? null : new
                    {
                        e.Result.FinishTime,
                        e.Result.Position
                    }
                })
                .ToListAsync();

            return Ok(list);
        }

        /// <summary>
        /// PUT /api/enrolments/{id}/status
        /// Organiser updates an enrolment's status (e.g., Confirmed, Cancelled).
        /// </summary>
        [HttpPut("{id:int}/status")]
        [RoleAuthorizeFilter("Organiser")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateEnrolmentStatusRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = HttpContext.Session.GetInt32("UserId");
            var organiser = await _db.OrganiserProfiles
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (organiser == null)
                return Unauthorized();

            var enrolment = await _db.Enrolments
                .Include(e => e.Category).ThenInclude(c => c!.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentId == id);

            if (enrolment == null)
                return NotFound(new { message = "Enrolment not found." });

            // Verify organiser owns the event
            if (enrolment.Category!.Event!.OrganiserId != organiser.OrganiserId)
                return StatusCode(403, new { message = "You do not own this event." });

            enrolment.Status = req.Status;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Enrolment status updated successfully." });
        }
    }
}