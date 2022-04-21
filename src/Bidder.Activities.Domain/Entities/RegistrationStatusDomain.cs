using System.Collections.Generic;
using Bidder.Activities.Domain.Exceptions;
using Bidder.Activities.Domain.Shared;
using FluentValidation;

namespace Bidder.Activities.Domain.Entities
{
    public class RegistrationStatusDomain
    {
      
        public long TenderId { get; }
        public List<ValidationError> ValidationResults { get; set; }
        public RegistrationStatusDomain(IValidator<RegistrationStatusDomain> validator,
            long tenderId)
        {
            TenderId = tenderId;
           
            ValidationResults = validator.Validate(this).MapToCustomError();
        }
    }
}
