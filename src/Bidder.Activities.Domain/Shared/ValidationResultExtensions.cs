using System;
using System.Collections.Generic;
using System.Linq;
using Bidder.Activities.Domain.Exceptions;
using FluentValidation.Results;

namespace Bidder.Activities.Domain.Shared;

public static class ValidationResultExtensions
{
    public static List<ValidationError> MapToCustomError(this ValidationResult validationResult)
    {
        return validationResult.Errors
            .Select(x => new ValidationError(Convert.ToInt32(x.ErrorCode), x.CustomState == null ? x.PropertyName : x.CustomState.ToString(), x.ErrorMessage, x.PropertyName))
            .ToList();
    }
}