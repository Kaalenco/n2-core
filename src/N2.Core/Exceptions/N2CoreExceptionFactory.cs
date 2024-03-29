using System.Diagnostics.CodeAnalysis;

namespace Dpi.Repository.Exceptions;

public class N2CoreExceptionFactory : IExceptionFactory
{
    [DoesNotReturn]
    public void ThrowCoreException() => throw CoreException;
    private static readonly N2CoreException _n2CoreException = new N2CoreException();
    public N2CoreException CoreException => _n2CoreException;

    private static readonly ConfigurationException _n2ConfigurationException = new ConfigurationException();
    public ConfigurationException ConfigurationException => _n2ConfigurationException;

    private static readonly UnauthorizedException _n2UnauthorizedException = new UnauthorizedException();
    public UnauthorizedException UnauthorizedException => _n2UnauthorizedException;

    [DoesNotReturn]
    public void ThrowUnauthorizedException(string message) => throw Unauthorized(message);
    public UnauthorizedException Unauthorized(string message)
    {
        return new UnauthorizedException(message);
    }

    [DoesNotReturn]
    public void ThrowConnectionStringNotFound(string name) => throw ConnectionStringNotFound(name);
    public ConfigurationException ConnectionStringNotFound(string name)
    {
        return new ConfigurationException($"Connection string not found: {name}");
    }

    [DoesNotReturn]
    public void ThrowDirectoryNotFoundException(string name) => throw DirectoryNotFoundException(name);
    public ConfigurationException DirectoryNotFoundException(string name)
    {
        return new ConfigurationException($"Directory not found: {name}");
    }

    [DoesNotReturn]
    public void ThrowElementNotFoundException(string name) => throw ElementNotFoundException(name);
    public ElementNotFoundException ElementNotFoundException(string name)
    {
        return new ElementNotFoundException(name);
    }

    private static readonly OperationException _n2OperationException = new OperationException();
    public OperationException OperationException => _n2OperationException;

    [DoesNotReturn]
    public void ThrowInvalidOperationException(string message) => throw InvalidOperationException(message);
    public OperationException InvalidOperationException(string message)
    {
        return new OperationException(message);
    }

    public void ThrowIfNull([NotNull] object? obj)
    {
        if (obj == null)
        {
            throw new N2CoreException();
        }
    }
}
