using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using Bidder.Activities.Api.Services;
using Bidder.Activities.Domain;
using Bidder.Activities.Domain.Entities;
using Bidder.Activities.Services.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bidder.Activities.Api.Controllers.V1
{
    [ApiController]
    [ApiVersion("1")]
    [Route("v1")]
    public class PlaceBidController : ControllerBase
    {
        private readonly RegistrationStatusService _registrationStatus;
        private readonly ITokenService _tokenService;
        private readonly IBiddingService _biddingService;

        public PlaceBidController(RegistrationStatusService registrationStatus, ITokenService tokenService, IBiddingService biddingService)
        {
            _biddingService = biddingService;
            _tokenService = tokenService;
            _registrationStatus = registrationStatus;
        }

        [HttpPost]
        [Route("place-bid")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ValidationFailedResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetStatus(PlaceBidRequest bidRequest)
        {
            var tokenDetails = _tokenService.GetTokenDetails();
            var registrationStatus = await _registrationStatus.GetRegistrationStatus(bidRequest.TenderId.Value, tokenDetails);
            if (registrationStatus is not { Status: Status.Approved })
                return Forbid();

            var biddingRequest = BiddingRequest(tokenDetails, bidRequest, registrationStatus);
            var biddingResponse = await _biddingService.PlaceBid(biddingRequest);
            return StatusCode((int)biddingResponse.StatusCode, biddingResponse.Response);
        }

        private BiddingRequest BiddingRequest(TokenDetails tokenDetails, PlaceBidRequest bidRequest,
            RegistrationStatus registrationStatusDetails)
        {
            return new BiddingRequest
            {
                ItemId = bidRequest.ItemId.Value,
                TenderId = bidRequest.TenderId.Value,
                Amount = bidRequest.BidAmount.Value,
                BuyerId = registrationStatusDetails.BuyerId,
                BuyerRef = registrationStatusDetails.BuyerRef,
                SourceId = int.Parse(tokenDetails.SourceId),
                MarketplaceUniqueCode = int.Parse(tokenDetails.MarketplaceUniqueCode),
                MarketplaceChannelCode = "PxbJJKWid1"
            };
        }
    }

    public class PlaceBidRequest
    {
        private const string RangeErrorMessageFormat = "The {0} field should be a positive number.";
        [Required]
        [Range(1, long.MaxValue, ErrorMessage = RangeErrorMessageFormat)]
        public long? ItemId { get; set; }

        [Required]
        [Range(1, long.MaxValue, ErrorMessage = RangeErrorMessageFormat)]
        public long? TenderId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = RangeErrorMessageFormat)]
        public decimal? BidAmount { get; set; }
    }
}
