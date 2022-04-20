using Bidder.Activities.Domain.Entities;

namespace Bidder.Activities.Api.Domain.Model
{
    public class GetStatusResponse
    {
        public GetStatusResponse(string bidderId, Status status, string cta)
        {
            BidderId = bidderId;
            Status = status;
            CTA = cta;
        }

        public GetStatusResponse(RegistrationStatus registrationStatus)
        {
            if (registrationStatus == null)
                return;
            BidderId = registrationStatus.BidderId;
            Status = registrationStatus.Status;
            CTA = registrationStatus.CTA;

        }

        public string BidderId { get; }
        public Status Status { get; } = Status.None;
        public string CTA { get; }
    }
}
