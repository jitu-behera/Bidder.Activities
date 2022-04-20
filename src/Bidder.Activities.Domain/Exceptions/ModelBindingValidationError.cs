using System.Collections.Generic;

namespace Bidder.Activities.Domain.Exceptions
{
    public class ModelBindingValidationError
    {
        public IList<ValidationError> ValidationResults { get; set; }
    }
}
