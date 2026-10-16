using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/results")]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _db;

        public ResultsController(RaceDayDbContext db)
        {
            _db = db;
        }

        // =========================================================
        // ORGANISER ONLY - Capture result
        // =========================================================

        /// <summary>
        /// POST /api/results
        /// Organiser captures (or updates) a finish time and position for an enrolment.
        /// Only allowed if the organiser owns the event connected to the enrolment.
        /// </summary>
        [HttpPost]
        [RoleAuthorizeFilter("Organiser")]
        public async Task<IActionResult> Create([FromBody] CreateResultRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = HttpContext.Session.GetInt32("UserId");
            var organiser = await _db.OrganiserProfiles
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (organiser == null)
                return Unauthorized();

            // Load enrolment + its category + the event (to check ownership)
            var enrolment = await _db.Enrolments
                .Include(e => e.Category).ThenInclude(c => c!.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentId == req.EnrolmentId);

            if (enrolment == null)
                return NotFound(new { message = "Enrolment not found." });

            // Ownership check
            if (enrolment.Category!.Event!.OrganiserId != organiser.OrganiserId)
                return StatusCode(403, new { message = "You do not own this event." });

            // Upsert: update if exists, else create
            var existing = await _db.Results
                .FirstOrDefaultAsync(r => r.EnrolmentId == req.EnrolmentId);

            if (existing != null)
            {
                existing.FinishTime = req.FinishTime;
                existing.Position = req.Position;
                existing.Notes = req.Notes;
                existing.RecordedAt = DateTime.UtcNow;
            }
            else
            {
                _db.Results.Add(new Result
                {
                    EnrolmentId = req.EnrolmentId,
                    FinishTime = req.FinishTime,
                    Position = req.Position,
                    Notes = req.Notes
                });
            }

            await _db.SaveChangesAsync();

            return Ok(new { message = "Result captured successfully." });
        }

        // =========================================================
        // PARTICIPANT ONLY - View own results
        // =========================================================

        /// <summary>
        /// GET /api/results/mine
        /// Participant views their own results across all events.
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

            var results = await _db.Results
                .Where(r => r.Enrolment!.ParticipantId == participant.ParticipantId)
                .Include(r => r.Enrolment).ThenInclude(e => e!.Category).ThenInclude(c => c!.Event)
                .Select(r => new ResultDto
                {
                    ResultId = r.ResultId,
                    EnrolmentId = r.EnrolmentId,
                    FinishTime = r.FinishTime,
                    Position = r.Position,
                    RecordedAt = r.RecordedAt,
                    EventName = r.Enrolment!.Category!.Event!.Name,
                    CategoryName = r.Enrolment.Category.Name,
                    ParticipantName = ""
                })
                .OrderByDescending(r => r.RecordedAt)
                .ToListAsync();

            return Ok(results);
        }

        // =========================================================
        // ORGANISER ONLY - View all results for one event
        // =========================================================

        /// <summary>
        /// GET /api/results/event/{eventId}
        /// Organiser views all results for one of their own events, ordered by position.
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

            var owns = await _db.Events
                .AnyAsync(e => e.EventId == eventId
                            && e.OrganiserId == organiser.OrganiserId);

            if (!owns)
                return NotFound(new { message = "Event not found or you do not own it." });

            var results = await _db.Results
                .Where(r => r.Enrolment!.Category!.EventId == eventId)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Participant)
                    .ThenInclude(p => p!.User)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Category)
                .Select(r => new ResultDto
                {
                    ResultId = r.ResultId,
                    EnrolmentId = r.EnrolmentId,
                    FinishTime = r.FinishTime,
                    Position = r.Position,
                    RecordedAt = r.RecordedAt,
                    EventName = r.Enrolment!.Category!.Event!.Name,
                    CategoryName = r.Enrolment.Category.Name,
                    ParticipantName = r.Enrolment.Participant!.User!.FirstName
                                    + " " + r.Enrolment.Participant.User.LastName
                })
                .OrderBy(r => r.Position)
                .ToListAsync();

            return Ok(results);
        }
    }
}