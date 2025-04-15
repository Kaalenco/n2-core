namespace N2.Core;

public class NullLoggingService : ILogService
{
    public void LogCritical<T>(string message) => LogNothing();
    public void LogDebug<T>(string message) => LogNothing();
    public void LogError<T>(string message) => LogNothing();
    public void LogEvent<T>(string message, string category) => LogNothing();
    public void LogInformation<T>(string message) => LogNothing();
    public void LogWarning<T>(string message) => LogNothing();

    private static void LogNothing()
    {
        // Do nothing
    }
}
