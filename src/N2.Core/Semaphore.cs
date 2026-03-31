using System.Diagnostics;

namespace N2.Core;

public class Semaphore
{
    public Semaphore(int initialValue, string? name = null)
    {
        InitialValue = initialValue;
        CurrentValue = initialValue;
        MaxValue = initialValue + 1;
        Name = name ?? Guid.NewGuid().ToString();
    }

    public Semaphore(int initialValue)
    {
        InitialValue = initialValue;
        CurrentValue = initialValue;
        MaxValue = initialValue + 1;
        Name = Guid.NewGuid().ToString();
    }

    public Semaphore(int initialValue, int maxValue, string? name = null)
    {
        if (maxValue <= initialValue)
        {
            throw new ArgumentOutOfRangeException(nameof(maxValue), "Maxvalue should be larger than the initial value");
        }

        InitialValue = initialValue;
        CurrentValue = initialValue;
        MaxValue = maxValue;
        Name = name ?? Guid.NewGuid().ToString();
    }
    private readonly object _lock = new();
    public int InitialValue { get; private set; }
    public int MaxValue { get; private set; }
    public int CurrentValue { get; private set; }
    public string Name { get; private set; }

    public bool WaitOne(TimeSpan timeSpan)
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();
        while (true)
        {
            lock (_lock)
            {
                if (CurrentValue < MaxValue)
                {
                    CurrentValue+=1;
                    return true;
                }
            }
            if (CurrentValue > MaxValue)
            {
                CurrentValue -= 1;
                if (stopwatch.Elapsed > timeSpan)
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }
    }

    public void Release()
    {
        lock (_lock)
        {
            CurrentValue -= 1;
        }
    }
}
