namespace N2.Core.Exceptions;

public class N2CoreExceptionFactory : IExceptionFactory
{
    public void ThrowCoreException() => throw CoreException;
    private static readonly N2CoreException _n2CoreException = new();
    public N2CoreException CoreException => _n2CoreException;

    private static readonly ConfigurationException _n2ConfigurationException = new();
    public ConfigurationException ConfigurationException => _n2ConfigurationException;

    private static readonly UnauthorizedException _n2UnauthorizedException = new();
    public UnauthorizedException UnauthorizedException => _n2UnauthorizedException;

    public void ThrowUnauthorizedException(string message) => throw Unauthorized(message);
    public UnauthorizedException Unauthorized(string message)
    {
        return new UnauthorizedException(message);
    }

    public void ThrowConnectionStringNotFound(string name) => throw ConnectionStringNotFound(name);
    public ConfigurationException ConnectionStringNotFound(string name)
    {
        return new ConfigurationException($"Connection string not found: {name}");
    }

    public void ThrowDirectoryNotFoundException(string name) => throw DirectoryNotFoundException(name);
    public ConfigurationException DirectoryNotFoundException(string name)
    {
        return new ConfigurationException($"Directory not found: {name}");
    }

    public void ThrowElementNotFoundException(string name) => throw ElementNotFoundException(name);
    public ElementNotFoundException ElementNotFoundException(string name)
    {
        return new ElementNotFoundException(name);
    }

    private static readonly OperationException _n2OperationException = new();
    public OperationException OperationException => _n2OperationException;

    public void ThrowInvalidOperationException(string message) => throw InvalidOperationException(message);
    public OperationException InvalidOperationException(string message)
    {
        return new OperationException(message);
    }

    public void ThrowIfNull(object? obj)
    {
        if (obj == null)
        {
            throw new N2CoreException();
        }
    }
}
