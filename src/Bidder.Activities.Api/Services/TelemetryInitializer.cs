using Bidder.Activities.CorrelationId;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Bidder.Activities.Api.Services
{
    public class TelemetryInitializer : ITelemetryInitializer
    {
        private const string CorrelationIdKey = "x-ba-correlation-id";
        private readonly ICorrelationIdProvider _correlationIdProvider;

        public TelemetryInitializer(ICorrelationIdProvider correlationIdProvider)
        {
            _correlationIdProvider = correlationIdProvider;
        }
        public void Initialize(ITelemetry telemetry)
        {
            var propTelemetry = (ISupportProperties)telemetry;
            if (!propTelemetry.Properties.ContainsKey(CorrelationIdKey))
            {
                propTelemetry.Properties.Add(CorrelationIdKey, _correlationIdProvider.GetCorrelationId());
            }
        }
    }
}
