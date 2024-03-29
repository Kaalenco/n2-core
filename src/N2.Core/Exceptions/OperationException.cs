namespace Dpi.Repository.Exceptions;

public class OperationException : N2CoreException
{
    public OperationException(string message) : base(message)
    {
        ErrorCode = 405;
    }

    public OperationException()
    {
        ErrorCode = 405;
    }

    public OperationException(string message, Exception innerException) : base(message, innerException)
    {
        ErrorCode = 405;
    }
}
