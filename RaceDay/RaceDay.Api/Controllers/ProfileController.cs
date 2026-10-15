using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Filters;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/profile")]
    [SessionAuthFilter]   // ⭐ Requires login for ALL endpoints
    public class ProfileController : ControllerBase
    {
        private readonly RaceDayDbContext _db;

        public ProfileController(RaceDayDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// GET /api/profile/me
        /// Returns the currently authenticated user's full profile.
        /// Both Organisers and Participants can use this.
        /// </summary>
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Unauthorized();

            var user = await _db.Users
                .Include(u => u.OrganiserProfile)
                .Include(u => u.ParticipantProfile)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound(new { message = "User not found." });

            var dto = new ProfileDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                ProfilePictureUrl = user.ProfilePictureUrl,
                OrganisationName = user.OrganiserProfile?.OrganisationName,
                PhoneNumber = user.OrganiserProfile?.PhoneNumber
                              ?? user.ParticipantProfile?.PhoneNumber,
                EmergencyContact = user.ParticipantProfile?.EmergencyContact
            };

            return Ok(dto);
        }

        /// <summary>
        /// PUT /api/profile/me
        /// Updates the currently authenticated user's profile.
        /// Only updates fields that apply to their role.
        /// </summary>
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Unauthorized();

            var user = await _db.Users
                .Include(u => u.OrganiserProfile)
                .Include(u => u.ParticipantProfile)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound();

            // Update base user fields
            user.FirstName = req.FirstName;
            user.LastName = req.LastName;
            if (!string.IsNullOrEmpty(req.ProfilePictureUrl))
                user.ProfilePictureUrl = req.ProfilePictureUrl;

            // Update role-specific fields
            if (user.Role == "Organiser" && user.OrganiserProfile != null)
            {
                user.OrganiserProfile.OrganisationName = req.OrganisationName;
                user.OrganiserProfile.PhoneNumber = req.PhoneNumber;
            }
            else if (user.Role == "Participant" && user.ParticipantProfile != null)
            {
                user.ParticipantProfile.PhoneNumber = req.PhoneNumber;
                user.ParticipantProfile.EmergencyContact = req.EmergencyContact;
            }

            await _db.SaveChangesAsync();

            // Refresh session name (in case FirstName/LastName changed)
            HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");

            return Ok(new { message = "Profile updated successfully." });
        }
    }
}