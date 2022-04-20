using System.Linq;
using Bidder.Activities.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Bidder.Activities.Services.Services
{
    public class TokenService : ITokenService
    {
        private IHttpContextAccessor _httpContextAccessor;
        private const string BearerPrefix = "Bearer ";

        public TokenService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public TokenDetails GetTokenDetails()
        {
            var userClaims = _httpContextAccessor.HttpContext.User.Claims;
            var claimsDictionary = userClaims.ToDictionary(x => x.Type);
            return new TokenDetails
            {
                CustomerId = claimsDictionary["ext_customer_id"].Value,
                MarketplaceId = claimsDictionary["marketplace_Id"].Value,
                PlatformId = claimsDictionary["platform_id"].Value
            };
        }
    }
}
