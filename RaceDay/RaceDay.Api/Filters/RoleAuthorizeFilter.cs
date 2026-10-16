using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Api.Filters
{
    /// <summary>
    /// Restricts an endpoint to a specific role.
    /// Returns:
    ///   401 if not logged in
    ///   403 if logged in but wrong role
    /// </summary>
    public class RoleAuthorizeFilter : ActionFilterAttribute
    {
        private readonly string _requiredRole;

        public RoleAuthorizeFilter(string requiredRole)
        {
            _requiredRole = requiredRole;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    message = "Authentication required. Please log in."
                });
                return;
            }

            var role = context.HttpContext.Session.GetString("UserRole");
            if (role != _requiredRole)
            {
                context.Result = new ObjectResult(new
                {
                    message = $"Access denied. This endpoint is for {_requiredRole}s only."
                })
                {
                    StatusCode = 403
                };
            }

            base.OnActionExecuting(context);
        }
    }
}