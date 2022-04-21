namespace Bidder.Activities.Domain.Entities
{
    public class RegistrationStatus
    {
        public string BuyerId { get; set; }
        public Status Status { get; set; } = Status.None;
        public string CTA { get; set; }
        public string BuyerRef { get; set; }
    }
}
