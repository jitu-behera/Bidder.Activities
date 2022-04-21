using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Bidder.Activities.Api.Domain.Model;
using Bidder.Activities.Api.Services;
using Bidder.Activities.Domain.Entities;
using FluentValidation;

namespace Bidder.Activities.Api.Controllers.V1
{
    [ApiController]
    [ApiVersion("1")]
    [Route("v1")]
    public class BuyerRegistrationController : ControllerBase
    {
        private readonly IValidator<RegistrationStatusDomain> _registrationStatusValidator;
        private readonly RegistrationStatusService _registrationStatusService;
        public BuyerRegistrationController(
            IValidator<RegistrationStatusDomain> registrationStatusValidator,
            RegistrationStatusService registrationStatusService)
        {
            _registrationStatusValidator = registrationStatusValidator;
            _registrationStatusService = registrationStatusService;
        }

        [HttpGet]
        [Route("tender/{id:long}/bidder/me")]
        [ProducesResponseType(typeof(GetStatusResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ValidationFailedResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetStatus([FromRoute] long id)
        {
            var registrationStatus = new RegistrationStatusDomain(_registrationStatusValidator, id);
            if (registrationStatus.ValidationResults.Any())
            {
                return BadRequest(new ValidationFailedResponse(registrationStatus.ValidationResults));
            }

            var statusDetails = await _registrationStatusService.GetRegistrationStatusDetails(registrationStatus.TenderId).ConfigureAwait(false);
            return Ok(new GetStatusResponse(statusDetails));
        }


    }
}
