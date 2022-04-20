using System;
using System.IO;
using System.Threading.Tasks;
using Bidder.Activities.Api.Services;
using Microsoft.AspNetCore.Http;
using RequestResponseLogger.Configuration;

namespace Bidder.Activities.Api.Application.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly IRequestResponseLogger _requestResponseLogger;
        private readonly RequestDelegate _next;
        private readonly LoggingConfiguration _logConfiguration;
        private const string Health = "/health";

        public RequestResponseLoggingMiddleware(RequestDelegate next, IRequestResponseLogger requestResponseLogger, LoggingConfiguration logConfiguration)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _requestResponseLogger = requestResponseLogger;
            _logConfiguration = logConfiguration;

        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Path.ToString().ToLower().EndsWith(Health) && _logConfiguration.IsLoggingOn && !_requestResponseLogger.IsExcludeLogRoute(_logConfiguration.ExcludeLogRoutes))
            {
                var routeName = context.Request.Path;
                await _requestResponseLogger.LogRequest(context, routeName);
                await LogResponse(context, routeName);
            }
            else
            {
                await _next(context);
            }


        }

        private async Task LogResponse(HttpContext context, PathString routeName)
        {
            var originalBodyStream = context.Response.Body;
            try
            {
                await using var memoryStream = new MemoryStream();
                context.Response.Body = memoryStream;

                await _next(context);

                memoryStream.Position = 0;
                await _requestResponseLogger.LogResponse(memoryStream, originalBodyStream, routeName);
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }
    }
}
