using Polly;
using Polly.Timeout;
using Serilog;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bidder.Activities.Api.Application.Policies
{
    public static partial class Polly
    {
        public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
        {
            return Policy
                .TimeoutAsync<HttpResponseMessage>(PollySettings.TimeoutPolicy.TimeoutPeriodInSeconds,
                    TimeoutStrategy.Pessimistic,
                    (context, timespan, task) =>
                    {
                        Log.Warning(
                            $"{context.PolicyKey} at {context.OperationKey}: execution Timed out after {timespan.TotalSeconds} seconds.");
                        return Task.CompletedTask;
                    });
        }
    }
}