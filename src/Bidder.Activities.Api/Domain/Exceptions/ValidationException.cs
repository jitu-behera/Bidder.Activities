using System;
using System.Collections.Generic;
using System.Linq;
using Bidder.Activities.Domain.Exceptions;

namespace Bidder.Activities.Api.Domain.Exceptions
{
    /// <summary>
    /// Custom exception thrown from ValidationBehavior in case of a fluent validation error.
    /// Handled globally in ExceptionMiddleware.
    /// </summary>
    public class ValidationException : Exception
    {
        public IReadOnlyList<ValidationError> Errors { get; set; }

        public ValidationException(string message) : base(message)
        {
        }

        public ValidationException(IEnumerable<ValidationError> errors, string message = nameof(ValidationException)) :
            this(message)
        {
            Errors = errors?.ToArray();
        }
    }
}