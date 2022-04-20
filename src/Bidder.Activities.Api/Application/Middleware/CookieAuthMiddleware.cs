using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Bidder.Activities.Api.Application.Middleware
{
    public class CookieAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public CookieAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var request = httpContext.Request;
            if (request.Cookies.ContainsKey("ba_widget_auth_token"))
            {
                if (!request.Headers.ContainsKey("Authorization"))
                {
                    string token = request.Cookies["ba_widget_auth_token"];
                    request.Headers.Add("Authorization", $"Bearer {token}");
                }
            }

            await _next(httpContext);
        }
    }
}