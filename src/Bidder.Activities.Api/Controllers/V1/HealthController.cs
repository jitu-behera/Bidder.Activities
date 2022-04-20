using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Bidder.Activities.Api.Application.Handlers;
using Microsoft.AspNetCore.Authorization;

namespace Bidder.Activities.Api.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v1")]
    [ExcludeFromCodeCoverage]
    [AllowAnonymous]
    public class HealthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public HealthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Health check endpoint
        /// This health endpoint will ensure health for all the external services that the service is using which are not registered with Service Discovery.
        /// </summary>
        /// <response code="200"> Success </response>
        [HttpGet]
        [Route("health")]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        public async Task<ActionResult> HealthCheck()
        {
            var healthStatus = await _mediator.Send(new HealthCheck());
            return Ok(healthStatus);
        }
    }
}