using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers
{
    public class ParticipantController : Controller
    {
        protected readonly RaceDayApiClient Api;
        public ParticipantController(RaceDayApiClient api) => Api = api;

        protected bool IsParticipant() =>
            HttpContext.Session.GetInt32("UserId").HasValue &&
            HttpContext.Session.GetString("Role") == "Participant";

        // 1. LIST ALL EVENTS
        public async Task<IActionResult> Index()
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Auth");
            ViewBag.DisplayName = HttpContext.Session.GetString("DisplayName");

            var response = await Api.SendAsync(HttpMethod.Get, "api/events");
            var events = await Api.ReadAsync<List<EventDto>>(response) ?? new List<EventDto>();

            // Only show upcoming events
            //events = events.Where(e => e.EventDate >= DateTime.Today).OrderBy(e => e.EventDate).ToList();
            events = events.OrderBy(e => e.EventDate).ToList();

            return View(events);
        }

        // 2. EVENT DETAILS
        public async Task<IActionResult> EventDetails(int id)
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Auth");

            // Get event info
            var evtRes = await Api.SendAsync(HttpMethod.Get, $"api/events/{id}");
            var evt = await Api.ReadAsync<EventDto>(evtRes);
            if (evt == null) return NotFound();

            // Get categories
            var catRes = await Api.SendAsync(HttpMethod.Get, $"api/events/{id}/categories");
            ViewBag.Categories = await Api.ReadAsync<List<CategoryDto>>(catRes) ?? new List<CategoryDto>();

            return View(evt);
        }

        // 3. ENROL IN EVENT
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EnterEvent(int eventId, int categoryId)
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Auth");

            var req = new CreateEnrolmentRequest { CategoryId = categoryId };
            var response = await Api.SendAsync(HttpMethod.Post, "api/enrolments", req);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "You have successfully enrolled in the event!";
            }
            else
            {
                TempData["Error"] = "Failed to enrol. You might already be enrolled in this category.";
            }

            return RedirectToAction(nameof(EventDetails), new { id = eventId });
        }

        // 4. MY EVENTS
        public async Task<IActionResult> MyEvents()
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Auth");
            var response = await Api.SendAsync(HttpMethod.Get, "api/enrolments/mine");
            var enrolments = await Api.ReadAsync<List<EnrolmentDto>>(response) ?? new List<EnrolmentDto>();
            return View(enrolments);
        }

        // 5. MY RESULTS
        public async Task<IActionResult> MyResults()
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Auth");
            var response = await Api.SendAsync(HttpMethod.Get, "api/results/mine");
            var results = await Api.ReadAsync<List<ResultDto>>(response) ?? new List<ResultDto>();
            return View(results);
        }

        // 6. PROFILE
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Auth");
            var response = await Api.SendAsync(HttpMethod.Get, "api/profile/me");
            var profile = await Api.ReadAsync<ProfileDto>(response);
            return View(profile);
        }
    }
}