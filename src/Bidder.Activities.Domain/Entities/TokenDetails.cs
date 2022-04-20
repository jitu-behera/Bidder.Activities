using System.Text.Json.Serialization;

namespace Bidder.Activities.Domain.Entities
{
    public class TokenDetails
    {
        public string PlatformId { get; set; }
        public string CustomerId { get; set; }
        public string MarketplaceId { get; set; }
    }

}
