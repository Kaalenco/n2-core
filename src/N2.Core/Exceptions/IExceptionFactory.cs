using System.Diagnostics.CodeAnalysis;

namespace Dpi.Repository.Exceptions;

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

    [DoesNotReturn]
    void ThrowInvalidOperationException(string message);
    [DoesNotReturn]
    void ThrowCoreException();
    [DoesNotReturn]
    void ThrowConnectionStringNotFound(string name);
    [DoesNotReturn]
    void ThrowDirectoryNotFoundException(string name);
    [DoesNotReturn]
    void ThrowElementNotFoundException(string name);
    [DoesNotReturn]
    void ThrowUnauthorizedException(string message);

    void ThrowIfNull([NotNull] object? obj);
}
