using System.Threading.Tasks;
using Bidder.Activities.Domain.Entities;
using Bidder.Activities.Services.Services;

namespace Bidder.Activities.Api.Services
{
    public class RegistrationStatusService
    {
        private readonly ITokenService _tokenService;
        private readonly IRegistrationStatusRepository _registrationStatusRepository;

        public RegistrationStatusService(ITokenService tokenService, IRegistrationStatusRepository registrationStatusRepository)
        {
            _tokenService = tokenService;
            _registrationStatusRepository = registrationStatusRepository;
        }

        public Task<RegistrationStatus> GetRegistrationStatusDetails(long auctionId)
        {
            var tokenDetails = _tokenService.GetTokenDetails();
            return GetRegistrationStatus(auctionId, tokenDetails);
        }

        public Task<RegistrationStatus> GetRegistrationStatus(long auctionId, TokenDetails tokenDetails)
        {
            var id = $"{auctionId}-{tokenDetails.CustomerId}-{tokenDetails.MarketplaceId}";
            var partitionKey = $"{tokenDetails.CustomerId}-{tokenDetails.MarketplaceId}";
            return _registrationStatusRepository.GetItemAsync(id, partitionKey);
        }
    }
}
