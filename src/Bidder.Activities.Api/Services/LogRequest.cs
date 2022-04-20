namespace Bidder.Activities.RequestResponseLogger
{
    public class LogRequest
    {
        public string MethodName { get; set; }
        public string Message { get; set; }
        public string CorrelationId { get; set; }
        public dynamic AdditionalDetails { get; set; }
    }
}
