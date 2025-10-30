using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Sistema.Security
{
    public class AuthorizeAdminAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var role = context.HttpContext.Session.GetString("Role");
            var token = context.HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(token))
            {
                context.Result = new RedirectToActionResult("Login", "Account", new { area = "" });
                return;
            }

            if (role != "Admin")
            {
                context.Result = new RedirectToActionResult("Login", "Account", new { area = "" });
                return;
            }
        }
    }
}
