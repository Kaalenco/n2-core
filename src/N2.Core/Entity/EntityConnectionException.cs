using N2.Core.Commands;
using N2.Core.Exceptions;

namespace N2.Core.Entity;

public class EntityConnectionException : N2CoreException
{
    public int ErrorCode { get; protected set; } = 500;

    public EntityConnectionException(string message) : base(ResponseStatus.ServiceUnavailable, message)
    {
    }

    public EntityConnectionException() : base(ResponseStatus.ServiceUnavailable)
    {
    }

    public EntityConnectionException(string message, Exception innerException) : base(ResponseStatus.ServiceUnavailable, message, innerException)
    {
    }
}