using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers
{
    public class AuthController : Controller
    {
        private readonly RaceDayApiClient _api;
        public AuthController(RaceDayApiClient api) => _api = api;

        [HttpGet] public IActionResult Register() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVm vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var req = new RegisterRequest { FirstName = vm.FirstName, LastName = vm.LastName, Email = vm.Email, Password = vm.Password, Role = vm.Role, OrganisationName = vm.OrganisationName, PhoneNumber = vm.PhoneNumber, EmergencyContact = vm.EmergencyContact };
            var response = await _api.SendAsync(HttpMethod.Post, "api/auth/register", req);
            if (!response.IsSuccessStatusCode) { ModelState.AddModelError("", "Registration failed. Email may be in use."); return View(vm); }
            TempData["Success"] = "Registration successful. Please log in.";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet] public IActionResult Login() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVm vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var response = await _api.SendAsync(HttpMethod.Post, "api/auth/login", new LoginRequest { Email = vm.Email, Password = vm.Password });
            if (!response.IsSuccessStatusCode) { ModelState.AddModelError("", "Invalid email or password."); return View(vm); }
            var auth = await _api.ReadAsync<AuthResponse>(response);
            if (auth == null) { ModelState.AddModelError("", "Could not read login response."); return View(vm); }
            var apiCookie = RaceDayApiClient.ExtractSessionCookie(response);
            if (!string.IsNullOrWhiteSpace(apiCookie)) HttpContext.Session.SetString("ApiSessionCookie", apiCookie);
            HttpContext.Session.SetInt32("UserId", auth.UserId);
            HttpContext.Session.SetString("Role", auth.Role);
            HttpContext.Session.SetString("DisplayName", $"{auth.FirstName} {auth.LastName}");
            return auth.Role == "Organiser" ? RedirectToAction("Index", "Organiser") : RedirectToAction("Index", "Participant");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _api.SendAsync(HttpMethod.Post, "api/auth/logout");
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}