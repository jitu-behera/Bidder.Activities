using System;
using System.Threading.Tasks;
using Bidder.Activities.CorrelationId;
using Microsoft.AspNetCore.Http;

namespace Bidder.Activities.Api.Application.Middleware
{
    public class SetCorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeader = "x-br-correlation-id";

        public SetCorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext httpContext, ICorrelationIdManager correlationIdManager)
        {
            var correlationId = correlationIdManager.InitializeCorrelationId();
            httpContext.Response.Headers.Add(CorrelationIdHeader, correlationId);

            await _next(httpContext);
        }
    }
}
