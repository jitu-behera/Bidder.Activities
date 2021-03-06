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

        public Task<RegistrationStatus> GetRegistrationStatusDetails(long TenderId)
        {
            var tokenDetails = _tokenService.GetTokenDetails();
            return GetRegistrationStatus(TenderId, tokenDetails);
        }

        public Task<RegistrationStatus> GetRegistrationStatus(long TenderId, TokenDetails tokenDetails)
        {
            var id = $"{TenderId}-{tokenDetails.CustomerId}-{tokenDetails.MarketplaceUniqueCode}";
            var partitionKey = $"{tokenDetails.CustomerId}-{tokenDetails.MarketplaceUniqueCode}";
            return _registrationStatusRepository.GetItemAsync(id, partitionKey);
        }
    }
}
