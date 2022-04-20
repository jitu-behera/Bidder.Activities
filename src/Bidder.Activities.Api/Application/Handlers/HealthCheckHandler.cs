using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Bidder.Activities.Api.Domain.Model;
using MediatR;

namespace Bidder.Activities.Api.Application.Handlers
{
    [ExcludeFromCodeCoverage]
    public class HealthCheck : IRequest<string>
    {
    }

    /// <summary>
    /// Check health for all external services which are not managed by Service Discovery here.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class HealthCheckHandler : IRequestHandler<HealthCheck, string>
    {
        public async Task<string> Handle(HealthCheck request, CancellationToken cancellationToken)
        {
            // Write health check code here.
            await Task.CompletedTask;

            // Check current build number.
            return HealthCheckResponse.BuildVersion;
        }
    }
}