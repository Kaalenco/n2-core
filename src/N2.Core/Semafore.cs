namespace N2.Core;

public class Semafore
{
    private int count;
    private readonly object lockObject = new();

    public Semafore(int count)
    {
        this.count = count;
    }

    public void Release()
    {
        lock (lockObject)
        {
            count += 1;
            Monitor.Pulse(lockObject);
        }
    }

    public void Wait()
    {
        lock (lockObject)
        {
            while (count == 0)
            {
                Monitor.Wait(lockObject);
            }
            count -= 1;
        }
    }
}