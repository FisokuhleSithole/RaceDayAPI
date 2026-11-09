using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers
{
    public class ParticipantController : Controller
    {
        protected readonly RaceDayApiClient Api;
        public ParticipantController(RaceDayApiClient api) => Api = api;
        protected bool IsParticipant() => HttpContext.Session.GetInt32("UserId").HasValue && HttpContext.Session.GetString("Role") == "Participant";
        public IActionResult Index() { if (!IsParticipant()) return RedirectToAction("Login", "Auth"); ViewBag.DisplayName = HttpContext.Session.GetString("DisplayName"); return View(); }
    }
}