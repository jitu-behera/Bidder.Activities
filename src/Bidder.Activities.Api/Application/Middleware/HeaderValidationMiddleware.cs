using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Bidder.Activities.HttpHeaders;

public class HeaderValidationMiddleware
{
    private const string Health = "/health";
    private readonly RequestDelegate _next;
    public HeaderValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, RequiredHeaders requiredHeaders)
    {
        if (httpContext.Request.Path.ToString().ToLower().EndsWith(Health))
        {
            await _next(httpContext);
            return;
        }

        var missingHeaders = MissingHeaders(httpContext, requiredHeaders);
        if (missingHeaders.Any())
        {
            var responseBody = $"Missing required headers: {string.Join(", ", missingHeaders.Select(x => x.Key))}";
            httpContext.Response.StatusCode = 400;
            await httpContext.Response.WriteAsync(responseBody);
            return;
        }

        await _next(httpContext);
    }

    private static List<KeyValuePair<string, string>> MissingHeaders(HttpContext httpContext, RequiredHeaders requiredHeaders)
    {
        return requiredHeaders.Request
            .Select(key => new KeyValuePair<string, string>(key, httpContext.Request.Headers[key]))
            .Where(tracingHeader => string.IsNullOrWhiteSpace(tracingHeader.Value))
            .ToList();
    }

    
}


public class RequiredHeaders
{
    public IList<string> Request { get; set; } = new List<string>();
}