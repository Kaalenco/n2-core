namespace N2.Core;

public record WorkerStatus : IWorkerStatus
{
    public bool IsRunning { get; set; }
    public bool IsWaiting { get; set; }
    public bool ErrorOccured { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string? ErrorStackTrace { get; set; }
    public string? ErrorSource { get; set; }
    public int ErrorCode { get; set; }
    public string CurrentAction { get; set; } = string.Empty;
    public DateTime StartTimeUtc { get; set; }
    public DateTime RunStartedUtc { get; set; }
    public DateTime NextRunTimeUtc { get; set; }
    public TimeSpan ElapsedTime { get; set; }
}