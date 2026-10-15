using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Api.Services;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _db;

        public AuthController(RaceDayDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Registers a new user as either an Organiser or Participant.
        /// POST /api/auth/register
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if email already exists
            if (await _db.Users.AnyAsync(u => u.Email == req.Email))
                return Conflict(new { message = "Email already registered." });

            // Create the user
            var user = new AppUser
            {
                Email = req.Email,
                PasswordHash = PasswordHasher.HashPassword(req.Password),
                FirstName = req.FirstName,
                LastName = req.LastName,
                Role = req.Role
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Create role-specific profile
            if (req.Role == "Organiser")
            {
                _db.OrganiserProfiles.Add(new OrganiserProfile
                {
                    UserId = user.UserId,
                    OrganisationName = req.OrganisationName,
                    PhoneNumber = req.PhoneNumber
                });
            }
            else if (req.Role == "Participant")
            {
                _db.ParticipantProfiles.Add(new ParticipantProfile
                {
                    UserId = user.UserId,
                    PhoneNumber = req.PhoneNumber,
                    EmergencyContact = req.EmergencyContact
                });
            }

            await _db.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "User registered successfully.",
                userId = user.UserId,
                email = user.Email,
                role = user.Role
            });
        }

        /// <summary>
        /// Logs in a user and creates a server-side session.
        /// POST /api/auth/login
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email == req.Email);

            if (user == null || !PasswordHasher.VerifyPassword(req.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid email or password." });

            // Store user info in server-side session
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");

            return Ok(new AuthResponse
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role
            });
        }

        /// <summary>
        /// Logs out the current user by clearing the session.
        /// POST /api/auth/logout
        /// </summary>
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Logged out successfully." });
        }

        /// <summary>
        /// Returns the currently authenticated user's session info.
        /// Useful for the MVC front end in Part 3.
        /// GET /api/auth/me
        /// </summary>
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Unauthorized(new { message = "Not authenticated." });

            return Ok(new
            {
                userId,
                email = HttpContext.Session.GetString("UserEmail"),
                role = HttpContext.Session.GetString("UserRole"),
                name = HttpContext.Session.GetString("UserName")
            });
        }
    }
}