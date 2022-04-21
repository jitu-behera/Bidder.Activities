using Bidder.Activities.Domain.Entities;

namespace Bidder.Activities.Api.Domain.Model
{
    public class GetStatusResponse
    {
        public GetStatusResponse(string buyerId, Status status, string cta)
        {
            BuyerId = buyerId;
            Status = status;
            CTA = cta;
        }

        public GetStatusResponse(RegistrationStatus registrationStatus)
        {
            if (registrationStatus == null)
                return;
            BuyerId = registrationStatus.BuyerId;
            Status = registrationStatus.Status;
            CTA = registrationStatus.CTA;

        }

        public string BuyerId { get; }
        public Status Status { get; } = Status.None;
        public string CTA { get; }
    }
}
