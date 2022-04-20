
namespace Bidder.Activities.Api.Application.Policies
{
    public partial class Polly
    {
        public static PollySettings PollySettings { get; set; } = new PollySettings();
    }

    public class PollySettings
    {
        public RetryPolicy RetryPolicy { get; set; }
        public CircuitBreakerPolicy CircuitBreakerPolicy { get; set; }
        public TimeoutPolicy TimeoutPolicy { get; set; }
    }

    public class RetryPolicy
    {
        public int RetryCount { get; set; }
        public int SleepDurationInSeconds { get; set; }
    }

    public class CircuitBreakerPolicy
    {
        public int ConsecutiveFaultsCount { get; set; }
        public int BreakPeriodInSeconds { get; set; }
    }

    public class TimeoutPolicy
    {
        public int TimeoutPeriodInSeconds { get; set; }
    }
}