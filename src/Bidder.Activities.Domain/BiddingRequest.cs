namespace Bidder.Activities.Domain;

public class BiddingRequest
{
    public long LotId { get; set; }
    public long AuctionId { get; set; }
    public decimal Amount { get; set; }
    public string BidderId { get; set; }
    public string BidderRef { get; set; }
    public int PlatformId { get; set; }
    public int MarketplaceId { get; set; }
    public string MarketplaceChannelCode { get; set; }

    //| LotId | AuctionId | BidAmount | BidderId | PaddleNumber | PlatformId | MarketplaceId | MarketplaceChannelCode |
    //| 111   | 10        | 99.59     | 99       | 101C         | 20         | 201           | PxbJJKWid1                |
}