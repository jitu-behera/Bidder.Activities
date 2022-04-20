using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Bidder.Activities.Api.Application.Middleware;
using Bidder.Activities.Api.Services;
using Bidder.Activities.CorrelationId;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RequestResponseLogger.Configuration;

namespace Bidder.Activities.RequestResponseLogger
{
    [ExcludeFromCodeCoverage]
    public class RequestResponseLogger : IRequestResponseLogger
    {
        private readonly string _applicationName;
        private readonly string _applicationType;
        private readonly string _applicationVersion;
        private readonly string _localIpAddress;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;
        private readonly ICorrelationIdProvider _correlationIdProvider;

        public RequestResponseLogger(ILogger<RequestResponseLogger> logger, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, LoggingConfiguration logConfiguration, ICorrelationIdProvider correlationIdProvider)
        {
            _applicationName = logConfiguration.ApplicationName;
            _applicationVersion = logConfiguration.ApplicationVersion;
            _applicationType = configuration["ApplicationType"];
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _correlationIdProvider = correlationIdProvider;
            _localIpAddress = GetLocalIpAddress();
        }

        public void InsertLog(LogRequest request)
        {
            _logger.LogInformation(request.Message ?? $"Audit log from {_applicationType} - {_applicationName} - {_applicationVersion}.");
        }

        public async Task LogResponse(MemoryStream memoryStream, Stream originalBodyStream, string routeName)
        {
            using var reader = new StreamReader(memoryStream);
            var responseBody = await reader.ReadToEndAsync();

            // Copy body back to so its available to the user agent
            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(originalBodyStream);

            // Write response body to App Insights
            InsertLog(new LogRequest()
            {

                Message = $"Handled response for {routeName}",
                AdditionalDetails = responseBody,

                MethodName = nameof(RequestResponseLoggingMiddleware),
                CorrelationId = _correlationIdProvider.GetCorrelationId()
            });
        }

        public async Task LogRequest(HttpContext context, string routeName)
        {
            // Ensure the request body can be read multiple times
            context.Request.EnableBuffering();
            // Leave stream open so next middleware can read it
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, false, 512, true);
            var requestBody = await reader.ReadToEndAsync();
            // Reset stream position, so next middleware can read it
            context.Request.Body.Position = 0;
            InsertLog(new LogRequest()
            {
                Message = $"Handling request for {routeName}",
                AdditionalDetails = requestBody,
                MethodName = nameof(RequestResponseLoggingMiddleware),
                CorrelationId = _correlationIdProvider.GetCorrelationId()
            });
        }

        public bool IsExcludeLogRoute(string[] excludeLogRoutes)
        {
            return excludeLogRoutes.Any(route => _httpContextAccessor.HttpContext.Request.Path.ToString().ToLower().EndsWith(route));
        }

        /// <summary>
        /// Returns local ip address for logging.
        /// </summary>
        /// <returns>Ip address string</returns>
        private string GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            return host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString();
        }
    }
}
