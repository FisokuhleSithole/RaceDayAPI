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

        // --- DASHBOARD ---
        public async Task<IActionResult> Index()
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
            ViewBag.DisplayName = HttpContext.Session.GetString("DisplayName");
            var response = await Api.SendAsync(HttpMethod.Get, "api/events/mine");
            var events = await Api.ReadAsync<List<EventDto>>(response) ?? new List<EventDto>();
            return View(events);
        }

        // --- EVENTS CRUD ---
        public async Task<IActionResult> Events()
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
            var response = await Api.SendAsync(HttpMethod.Get, "api/events/mine");
            var events = await Api.ReadAsync<List<EventDto>>(response) ?? new List<EventDto>();
            return View(events);
        }

        [HttpGet]
        public IActionResult CreateEvent()
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
            return View(new EventFormVm());
        }

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
                ModelState.AddModelError("", "Failed to create event.");
                return View(vm);
            }

            //  Handle Banner Upload
            if (vm.BannerFile != null && vm.BannerFile.Length > 0)
            {
                var json = await response.Content.ReadAsStringAsync();
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                int newId = doc.RootElement.GetProperty("eventId").GetInt32();

                await Api.UploadFileAsync($"api/events/{newId}/banner", vm.BannerFile);
            }

            TempData["Success"] = "Event created!";
            return RedirectToAction(nameof(Events));
        }

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
                ModelState.AddModelError("", "Failed to update.");
                return View(vm);
            }
            TempData["Success"] = "Event updated!";
            return RedirectToAction(nameof(Events));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
            await Api.SendAsync(HttpMethod.Delete, $"api/events/{id}");
            TempData["Success"] = "Event deleted!";
            return RedirectToAction(nameof(Events));
        }

        // --- CATEGORIES ---
        public async Task<IActionResult> Categories(int eventId)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
            ViewBag.EventId = eventId;
            var evtRes = await Api.SendAsync(HttpMethod.Get, $"api/events/{eventId}");
            ViewBag.EventName = (await Api.ReadAsync<EventDto>(evtRes))?.Name ?? "Event";
            var response = await Api.SendAsync(HttpMethod.Get, $"api/events/{eventId}/categories");
            var categories = await Api.ReadAsync<List<CategoryDto>>(response) ?? new List<CategoryDto>();
            return View(categories);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(int eventId, string name, string description, decimal entryFee)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
            var req = new CreateCategoryRequest { Name = name, Description = description, EntryFee = entryFee };
            await Api.SendAsync(HttpMethod.Post, $"api/events/{eventId}/categories", req);
            TempData["Success"] = "Category added!";
            return RedirectToAction(nameof(Categories), new { eventId });
        }

        // --- ENROLMENTS ---
        public async Task<IActionResult> Enrolments(int eventId)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
            ViewBag.EventId = eventId;
            var evtRes = await Api.SendAsync(HttpMethod.Get, $"api/events/{eventId}");
            ViewBag.EventName = (await Api.ReadAsync<EventDto>(evtRes))?.Name ?? "Event";

            var response = await Api.SendAsync(HttpMethod.Get, $"api/enrolments/event/{eventId}");
            var enrolments = await Api.ReadAsync<List<dynamic>>(response) ?? new List<dynamic>();
            return View(enrolments);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEnrolmentStatus(int id, string status, int eventId)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
            await Api.SendAsync(HttpMethod.Put, $"api/enrolments/{id}/status", new { Status = status });
            TempData["Success"] = "Enrolment status updated!";
            return RedirectToAction(nameof(Enrolments), new { eventId });
        }

        // --- RESULTS ---
        public async Task<IActionResult> Results(int eventId)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
            ViewBag.EventId = eventId;
            var evtRes = await Api.SendAsync(HttpMethod.Get, $"api/events/{eventId}");
            ViewBag.EventName = (await Api.ReadAsync<EventDto>(evtRes))?.Name ?? "Event";
            var response = await Api.SendAsync(HttpMethod.Get, $"api/results/event/{eventId}");
            var results = await Api.ReadAsync<List<ResultDto>>(response) ?? new List<ResultDto>();
            return View(results);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CaptureResult(int enrolmentId, string finishTime, int position, int eventId)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
            var req = new CreateResultRequest { EnrolmentId = enrolmentId, FinishTime = finishTime, Position = position };
            await Api.SendAsync(HttpMethod.Post, "api/results", req);
            TempData["Success"] = "Result captured!";
            return RedirectToAction(nameof(Results), new { eventId });
        }
    }
}