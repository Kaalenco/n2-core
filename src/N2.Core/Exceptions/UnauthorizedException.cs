namespace Dpi.Repository.Exceptions;

public class UnauthorizedException : N2CoreException
{
    public UnauthorizedException(string message) : base(message)
    {
        ErrorCode = 401;
    }

    public UnauthorizedException()
    {
        ErrorCode = 401;
    }

    public UnauthorizedException(string message, Exception innerException) : base(message, innerException)
    {
        ErrorCode = 401;
    }
}