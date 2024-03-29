namespace N2.Core;

public class StdOutConsole : IStdOut
{
    public StdOutConsole(bool silent)
    {
        Silent = silent;
    }

    public bool Silent { get; set; }

    public void WriteLine(string message)
    {
        if (!Silent)
        {
            Console.WriteLine(message);
        }
    }

    public void Write(string message)
    {
        if (!Silent)
        {
            Console.Write(message);
        }
    }
}