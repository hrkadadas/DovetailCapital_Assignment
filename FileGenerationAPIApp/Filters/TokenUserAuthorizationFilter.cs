using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace FileGenerationAPIApp.Filters
{
    public class TokenUserAuthorizationFilter : IAsyncAuthorizationFilter
    {
        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var name = user.Identity?.Name ?? user.FindFirst("unique_name")?.Value ?? user.FindFirst("name")?.Value;
                var role = user.FindFirst(ClaimTypes.Role)?.Value ?? user.FindFirst("role")?.Value;

                if (id != null) context.HttpContext.Items["UserId"] = id;
                if (name != null) context.HttpContext.Items["Username"] = name;
                if (role != null) context.HttpContext.Items["Role"] = role;
            }
            return Task.CompletedTask;
        }
    }
}
