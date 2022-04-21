using Bidder.Activities.Domain.Entities;
using FluentValidation;

namespace Bidder.Activities.Domain.Validators.Fluent
{
    public class RegistrationStatusValidator : AbstractValidator<RegistrationStatusDomain>
    {
        public RegistrationStatusValidator()
        {
            RuleFor(m => m.TenderId)
                .GreaterThan(0)
                .WithCustom(ErrorCode.ERROR_INVALID_TenderId);
        }
    }

    public enum ErrorCode
    {
        ERROR_INVALID_TenderId = 1001,
    }
}
