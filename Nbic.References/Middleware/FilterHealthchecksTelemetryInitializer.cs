namespace Nbic.References.Middleware;

using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using System;

public class FilterHealthchecksTelemetryInitializer : ITelemetryInitializer
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public FilterHealthchecksTelemetryInitializer(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }
    public void Initialize(ITelemetry telemetry)
    {
        if ((_httpContextAccessor.HttpContext?.Request.Path.Value?.StartsWith("/hc", StringComparison.OrdinalIgnoreCase)).GetValueOrDefault())
        {
            // We don't want to track health checks in the metrics.
            if (telemetry is ISupportAdvancedSampling advancedSampling)
                advancedSampling.ProactiveSamplingDecision = SamplingDecision.SampledOut;

            // For the case that we cannot filter out the telemetry, we mark it as synthetic
            if (string.IsNullOrWhiteSpace(telemetry.Context.Operation.SyntheticSource))
                telemetry.Context.Operation.SyntheticSource = "HealthCheck";
        }
    }
}