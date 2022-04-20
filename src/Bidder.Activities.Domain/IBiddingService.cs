using System.Threading.Tasks;

namespace Bidder.Activities.Domain;

public interface IBiddingService
{
    Task<BiddingResponse> PlaceBid(BiddingRequest biddingRequest);
}