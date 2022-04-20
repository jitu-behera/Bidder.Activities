namespace Bidder.Activities.Domain.Entities
{
    public class RegistrationStatus
    {
        public string BidderId { get; set; }
        public Status Status { get; set; } = Status.None;
        public string CTA { get; set; }
        public string BidderRef { get; set; }
    }
}
