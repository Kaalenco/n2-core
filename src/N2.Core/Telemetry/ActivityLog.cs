using System.Diagnostics;

namespace N2.Core.Telemetry;

// Using the IActivityLogger interface from the Kaalenco.Common.Contracts namespace This class is a
// facade and uses the OpenTelemetry API.

/// <summary>
/// Logs activity using the OpenTelemetry API.
/// </summary>
public class ActivityLog : IActivityLogger
{
    private Activity? activity;
    private bool disposedValue;

    public ActivityLog()
    {
    }

    public ActivityLog(Activity? activity)
    {
        this.activity = activity;
    }

    public void SetActivity(Activity activity)
    {
        this.activity = activity;
    }

    public void Tag(string tag, object? value)
    {
        activity?.SetTag(tag, value ?? "NULL");
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                activity?.Dispose();
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public void TagError(Exception ex)
    {
        if (ex == null)
        {
            return;
        }
        activity?.AddEvent(new ActivityEvent(
            ex.GetType().Name,
            timestamp: DateTime.UtcNow,
            tags: new ActivityTagsCollection {
                { "Exception", ex.Message },
                { "StackTrace", ex.StackTrace }
                }
            ));
    }
}