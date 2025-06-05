namespace N2.Core.Exceptions;

public interface IExceptionFactory
{
    N2CoreException CoreException { get; }
    ConfigurationException ConfigurationException { get; }
    ConfigurationException ConnectionStringNotFound(string name);
    ConfigurationException DirectoryNotFoundException(string name);
    OperationException InvalidOperationException(string message);
    OperationException OperationException { get; }
    UnauthorizedException Unauthorized(string message);
    UnauthorizedException UnauthorizedException { get; }
    ElementNotFoundException ElementNotFoundException(string name);

    void ThrowInvalidOperationException(string message);
    void ThrowCoreException();
    void ThrowConnectionStringNotFound(string name);
    void ThrowDirectoryNotFoundException(string name);
    void ThrowElementNotFoundException(string name);
    void ThrowUnauthorizedException(string message);

    void ThrowIfNull(object? obj);
}
