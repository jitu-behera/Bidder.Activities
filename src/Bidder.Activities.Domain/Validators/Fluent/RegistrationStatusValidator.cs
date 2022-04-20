using Bidder.Activities.Domain.Entities;
using FluentValidation;

namespace Bidder.Activities.Domain.Validators.Fluent
{
    public class RegistrationStatusValidator : AbstractValidator<RegistrationStatusDomain>
    {
        public RegistrationStatusValidator()
        {
            RuleFor(m => m.AuctionId)
                .GreaterThan(0)
                .WithCustom(ErrorCode.ERROR_INVALID_AUCTIONID);
        }
    }

    public enum ErrorCode
    {
        ERROR_INVALID_AUCTIONID = 1001,
    }
}
