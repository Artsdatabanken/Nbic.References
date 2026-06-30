namespace Nbic.References.Middleware;

using OpenTelemetry;
using System.Diagnostics;

public class FilterHealthchecksProcessor : BaseProcessor<Activity>
{
    public override void OnEnd(Activity activity)
    {
        // Filter out health check requests
        if (activity?.DisplayName?.Contains("/hc") == true)
        {
            activity.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded;
            return;
        }

        // Filter out specific dependencies
        if (activity?.Kind == ActivityKind.Client &&
            activity?.GetTagItem("http.url")?.ToString()?.Contains("internal-service") == true)
        {
            activity.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded;
        }
    }
}