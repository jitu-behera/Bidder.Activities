using System.Linq;
using Bidder.Activities.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Bidder.Activities.Services.Services
{
    public class TokenService : ITokenService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

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
                MarketplaceUniqueCode = claimsDictionary["marketplace_Id"].Value,
                SourceId = claimsDictionary["platform_id"].Value
            };
        }
    }
}
