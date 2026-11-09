using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers
{
    public class OrganiserController : Controller
    {
        protected readonly RaceDayApiClient Api;
        public OrganiserController(RaceDayApiClient api) => Api = api;
        protected bool IsOrganiser() => HttpContext.Session.GetInt32("UserId").HasValue && HttpContext.Session.GetString("Role") == "Organiser";
        public IActionResult Index() { if (!IsOrganiser()) return RedirectToAction("Login", "Auth"); ViewBag.DisplayName = HttpContext.Session.GetString("DisplayName"); return View(); }
    }
}