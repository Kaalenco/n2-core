namespace N2.Core;

public class BackgroundWorker : IBackgroundWorker, IDisposable
{
    private readonly object LockObject = new();
    private bool IsActivated;
    private readonly Func<CancellationToken, Task> job;
    private readonly System.Timers.Timer restartTimer;
    private Task? currentTask;
    private bool disposedValue;
    private bool paused;
    private CancellationTokenSource? tokenSource;
    private WorkerStatus? workerStatus;
    private volatile bool stopped;

    public BackgroundWorker(
        int millisecondsInterval,
        bool autoStart,
        CancellationTokenSource? tokenSource,
        Func<CancellationToken, Task> job)
    {
        this.tokenSource = tokenSource;
        this.job = job;
        restartTimer = new System.Timers.Timer(millisecondsInterval);
        restartTimer.Elapsed += (sender, e) => DoWork();
        if (autoStart)
        {
            restartTimer.AutoReset = true;
            restartTimer.Start();
            // start the job immediately for the first time
            StartJob();
        }
        else
        {
            restartTimer.AutoReset = false;
            restartTimer.Stop();
        }
    }

    public IWorkerStatus CurrentStatus()
    {
        bool isRunning = IsRunning();
        bool isWaiting = IsWaiting();
        string action = isRunning ? "Running" : isWaiting ? "Waiting" : "Stopped";
        if (workerStatus == null)
        {
            workerStatus = InitializeWorkerStatus();
        }

        WorkerStatus result = workerStatus with
        {
            CurrentAction = action,
            IsRunning = isRunning,
            IsWaiting = isWaiting,
            ElapsedTime = DateTime.UtcNow - workerStatus.RunStartedUtc
        };
        return result;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public bool IsRunning() => restartTimer.AutoReset && JobActive() && (tokenSource == null || !tokenSource.IsCancellationRequested);

    public bool IsWaiting() => paused || (currentTask != null && currentTask.IsCompleted);

    public void PauseJob()
    {
        restartTimer.Stop();
        paused = true;
        if (workerStatus == null)
        {
            return;
        }

        workerStatus = workerStatus with
        {
            CurrentAction = "Paused"
        };
    }

    public void Restart()
    {
        stopped = false;
        restartTimer.AutoReset = true;
        restartTimer.Start();
        StartJob();
    }

    public void ContinueJob()
    {
        restartTimer.Start();
        paused = false;
        if (workerStatus == null)
        {
            return;
        }

        workerStatus = workerStatus with
        {
            CurrentAction = "Running"
        };
    }

    public void StartJob()
    {
        if (IsRunning())
        {
            return;
        }

        stopped = false;
        tokenSource = null;
        DoWork();
    }

    public void StopJob()
    {
        stopped = true;
        restartTimer.AutoReset = false;
        restartTimer.Stop();
        tokenSource?.Cancel();
        if (workerStatus == null)
        {
            return;
        }

        workerStatus = workerStatus with
        {
            CurrentAction = "Stopping"
        };
        currentTask?.Wait();
    }

    protected bool CanActivateJob()
    {
        lock (LockObject)
        {
            if (IsActivated)
            {
                return false;
            }

            IsActivated = true;
        }
        return true;
    }

    protected void DeactivateJob()
    {
        lock (LockObject)
        {
            IsActivated = false;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                tokenSource?.Cancel();
                currentTask?.Wait();
                tokenSource?.Dispose();
                currentTask?.Dispose();
                restartTimer?.Dispose();
            }

            disposedValue = true;
        }
    }

    private void DoWork()
    {
        if (stopped)
        {
            return;
        }

        if (tokenSource != null && tokenSource.IsCancellationRequested)
        {
            return;
        }

        if (JobActive())
        {
            return;
        }

        // not active, so start a new job run
        workerStatus ??= InitializeWorkerStatus() with
        {
            RunStartedUtc = DateTime.UtcNow,
            CurrentAction = "Running",
        };
        currentTask = Task.Run(Job);
    }

    private WorkerStatus InitializeWorkerStatus()
    {
        return new WorkerStatus
        {
            CurrentAction = "Running",
            ElapsedTime = TimeSpan.Zero,
            ErrorOccured = false,
            ErrorCode = 0,
            ErrorMessage = string.Empty,
            ErrorSource = string.Empty,
            ErrorStackTrace = string.Empty,
            IsRunning = IsRunning(),
            IsWaiting = IsWaiting(),
            NextRunTimeUtc = DateTime.UtcNow.AddMilliseconds(restartTimer.Interval),
            RunStartedUtc = DateTime.UtcNow,
            StartTimeUtc = DateTime.UtcNow
        };
    }

    private Task Job()
    {
        if (job == null)
        {
            return Task.CompletedTask;
        }

        return job(tokenSource?.Token ?? CancellationToken.None);
    }

    private bool JobActive()
    {
        if (currentTask == null)
        {
            return false;
        }

        return !currentTask.IsCompleted;
    }
}