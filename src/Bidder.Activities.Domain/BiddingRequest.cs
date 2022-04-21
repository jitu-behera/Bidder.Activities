namespace Bidder.Activities.Domain;

public class BiddingRequest
{
    public long ItemId { get; set; }
    public long TenderId { get; set; }
    public decimal Amount { get; set; }
    public string BuyerId { get; set; }
    public string BuyerRef { get; set; }
    public int SourceId { get; set; }
    public int MarketplaceUniqueCode { get; set; }
    public string MarketplaceChannelCode { get; set; }
}