using System.Diagnostics;

namespace N2.Core;

/// <summary>
/// Wait for an object to be available using a timeout.
/// </summary>
public sealed class WaitForObject : IDisposable
{
    private readonly Timer timer;
    private readonly DateTime endTime;
    private readonly Func<object?> condition;
    private readonly object timerLock = new();
    private bool Finished;
    private bool Timeout;
    private bool isDisposed;

    public WaitForObject(Func<object?> condition, int timeout = 30)
    {
        this.condition = condition;
        endTime = DateTime.Now.AddSeconds(timeout);
        timer = new Timer(Callback, null, 200, 200);
        Stopwatch sw = Stopwatch.StartNew();
    }

    public async Task<bool> WaitAsync()
    {
        while (!Finished)
        {
            await Task.Delay(100);
        }
        return !Timeout;
    }

    private void Callback(object? state)
    {
        if (DateTime.Now < endTime)
        {
            if (condition() != null)
            {
                lock (timerLock)
                {
                    Timeout = false;
                    Finished = true;
                }
            }
        }
        else
        {
            lock (timerLock)
            {
                Timeout = true;
                Finished = true;
            }
        }
    }

    private void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            if (disposing)
            {
                timer.Dispose();
            }
            isDisposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}