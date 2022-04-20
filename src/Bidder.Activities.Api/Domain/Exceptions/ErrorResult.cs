using System.Text.Json;

namespace Bidder.Activities.Api.Domain.Exceptions
{
    public class ErrorResult
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}