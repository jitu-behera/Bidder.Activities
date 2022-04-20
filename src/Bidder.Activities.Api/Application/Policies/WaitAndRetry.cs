using Polly;
using Polly.Extensions.Http;
using System;
using System.Net.Http;

namespace Bidder.Activities.Api.Application.Policies
{
    public static partial class Polly
    {
        public static IAsyncPolicy<HttpResponseMessage> GetWaitAndRetryPolicy()
        {
            var jitter = new Random();
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(PollySettings.RetryPolicy.RetryCount,
                    retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(PollySettings.RetryPolicy.SleepDurationInSeconds, retryAttempt))
                        + TimeSpan.FromMilliseconds(jitter.Next(0, 100))
                );
        }
    }
}