using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Api.Filters
{
    /// <summary>
    /// Requires the user to be logged in (UserId present in session).
    /// Returns 401 Unauthorized if not.
    /// </summary>
    public class SessionAuthFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    message = "Authentication required. Please log in."
                });
            }
            base.OnActionExecuting(context);
        }
    }
}