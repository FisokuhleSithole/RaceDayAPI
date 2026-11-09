using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers
{
    public class OrganiserController : Controller
    {
        protected readonly RaceDayApiClient Api;
        public OrganiserController(RaceDayApiClient api) => Api = api;

        protected bool IsOrganiser() =>
            HttpContext.Session.GetInt32("UserId").HasValue &&
            HttpContext.Session.GetString("Role") == "Organiser";

        // 1. DASHBOARD
        public async Task<IActionResult> Index()
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");

            ViewBag.DisplayName = HttpContext.Session.GetString("DisplayName");

            // Get my events for the dashboard
            var response = await Api.SendAsync(HttpMethod.Get, "api/events/mine");
            var events = await Api.ReadAsync<List<EventDto>>(response) ?? new List<EventDto>();

            return View(events);
        }

        // 2. LIST EVENTS
        public async Task<IActionResult> Events()
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");

            var response = await Api.SendAsync(HttpMethod.Get, "api/events/mine");
            var events = await Api.ReadAsync<List<EventDto>>(response) ?? new List<EventDto>();

            return View(events);
        }

        // 3. CREATE EVENT (GET)
        [HttpGet]
        public IActionResult CreateEvent()
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
            return View(new EventFormVm());
        }

        // 4. CREATE EVENT (POST)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(EventFormVm vm)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
            if (!ModelState.IsValid) return View(vm);

            var req = new CreateEventRequest
            {
                Name = vm.Name,
                Description = vm.Description,
                EventDate = vm.EventDate,
                Location = vm.Location,
                DistanceKm = vm.DistanceKm,
                EventType = vm.EventType,
                MaxParticipants = vm.MaxParticipants
            };

            var response = await Api.SendAsync(HttpMethod.Post, "api/events", req);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Failed to create event. Check your inputs.");
                return View(vm);
            }

            TempData["Success"] = "Event created successfully!";
            return RedirectToAction(nameof(Events));
        }

        // 5. EDIT EVENT (GET)
        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");

            var response = await Api.SendAsync(HttpMethod.Get, $"api/events/{id}");
            var evt = await Api.ReadAsync<EventDto>(response);

            if (evt == null) return NotFound();

            var vm = new EventFormVm
            {
                EventId = evt.EventId,
                Name = evt.Name,
                Description = evt.Description,
                EventDate = evt.EventDate,
                Location = evt.Location,
                DistanceKm = evt.DistanceKm,
                EventType = evt.EventType,
                MaxParticipants = evt.MaxParticipants
            };

            return View(vm);
        }

        // 6. EDIT EVENT (POST)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(int id, EventFormVm vm)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
            if (!ModelState.IsValid) return View(vm);

            var req = new UpdateEventRequest
            {
                Name = vm.Name,
                Description = vm.Description,
                EventDate = vm.EventDate,
                Location = vm.Location,
                DistanceKm = vm.DistanceKm,
                EventType = vm.EventType,
                MaxParticipants = vm.MaxParticipants
            };

            var response = await Api.SendAsync(HttpMethod.Put, $"api/events/{id}", req);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Failed to update event.");
                return View(vm);
            }

            TempData["Success"] = "Event updated successfully!";
            return RedirectToAction(nameof(Events));
        }

        // 7. DELETE EVENT
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");

            await Api.SendAsync(HttpMethod.Delete, $"api/events/{id}");
            TempData["Success"] = "Event deleted successfully!";

            return RedirectToAction(nameof(Events));
        }
    }
}