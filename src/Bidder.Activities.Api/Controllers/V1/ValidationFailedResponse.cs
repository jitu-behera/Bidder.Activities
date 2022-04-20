using System.Collections.Generic;
using Bidder.Activities.Domain.Exceptions;

namespace Bidder.Activities.Api.Controllers.V1;

public class ValidationFailedResponse
{
    public List<ValidationError> ValidationResults { get; }

    public ValidationFailedResponse(List<ValidationError> validationResults)
    {
        ValidationResults = validationResults;
    }
}