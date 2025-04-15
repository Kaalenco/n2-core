namespace Dpi.Repository.Exceptions;

public class N2CoreException : Exception
{
    public int ErrorCode { get; protected set; } = 500;

    public N2CoreException(string message) : base(message)
    {
    }

    public N2CoreException()
    {
    }

    public N2CoreException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
