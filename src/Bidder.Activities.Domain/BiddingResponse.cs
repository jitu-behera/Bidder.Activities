using System.Net;

namespace Bidder.Activities.Domain;

public class BiddingResponse
{
    public HttpStatusCode StatusCode { get; set; }
    public string Response { get; set; }
}