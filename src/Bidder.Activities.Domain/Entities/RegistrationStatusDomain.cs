using System.Collections.Generic;
using Bidder.Activities.Domain.Exceptions;
using Bidder.Activities.Domain.Shared;
using FluentValidation;

namespace Bidder.Activities.Domain.Entities
{
    public class RegistrationStatusDomain
    {
      
        public long AuctionId { get; }
        public List<ValidationError> ValidationResults { get; set; }
        public RegistrationStatusDomain(IValidator<RegistrationStatusDomain> validator,
            long auctionId)
        {
            AuctionId = auctionId;
           
            ValidationResults = validator.Validate(this).MapToCustomError();
        }
    }
}
