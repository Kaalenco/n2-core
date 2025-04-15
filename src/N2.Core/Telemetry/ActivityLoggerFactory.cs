using System.Diagnostics;

namespace N2.Core.Telemetry;

public class ActivityLoggerFactory : IActivityLoggerFactory
{
    public IActivityLogger StartActivity(string name)
    {
        Activity activity = new(name);
        activity.Start();
        return new ActivityLog(activity);
    }

    public IActivityLogger StartActivity<T>()
    {
        string? name = typeof(T).FullName;
        Activity activity = new(name!);
        activity.Start();
        return new ActivityLog(activity);
    }
}
