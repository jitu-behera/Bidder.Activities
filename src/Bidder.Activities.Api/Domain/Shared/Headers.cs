using System.Collections.Generic;

namespace Bidder.Activities.Api.Domain.Shared
{
    public class Headers
    {
        public IList<string> Request { get; set; } = new List<string>();

        public IDictionary<string, string> Response { get; set; } = new Dictionary<string, string>();
    }
}
