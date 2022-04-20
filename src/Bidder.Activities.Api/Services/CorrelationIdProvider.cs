using System;
using Microsoft.AspNetCore.Http;
using System.Text;
using Bidder.Activities.CorrelationId;

namespace Bidder.Activities.Api.Services
{
    public class CorrelationIdProvider : ICorrelationIdProvider, ICorrelationIdManager
    {
        private const string CorrelationIdKey = "x-ba-correlation-id";
        private readonly IHttpContextAccessor _accessor;
        private readonly CorrelationIdConfiguration _configuration;

        public CorrelationIdProvider(IHttpContextAccessor accessor, CorrelationIdConfiguration configuration)
        {
            _configuration = configuration;
            _accessor = accessor;
        }

        public string? GetCorrelationId()
        {
            var httpContext = _accessor.HttpContext;
            var httpContextItem = httpContext?.Items[CorrelationIdKey];
            return httpContextItem?.ToString();
        }

        public string InitializeCorrelationId()
        {
            string id = _accessor.HttpContext.Request.Headers.TryGetValue(CorrelationIdKey, out var correlationIdHeaderValues)
                ? correlationIdHeaderValues
                : NewCorrelationId();

            SetCorrelationId(id);
            return id;
        }


        private string NewCorrelationId()
        {
            var stringBuilder = new StringBuilder();
            return stringBuilder.Append(DateTimeOffset.Now.ToUnixTimeSeconds())
                .Append(_configuration.ServiceName)
                .Append(_configuration.DomainAcronym)
                .Append(Guid.NewGuid().ToString("N").Substring(0, 15))
                .ToString();
        }

        public void SetCorrelationId(string correlationId)
        {
            _accessor.HttpContext.Items[CorrelationIdKey] = correlationId;
        }
    }
}
