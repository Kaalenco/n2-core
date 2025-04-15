namespace N2.Core.Exceptions;

public class ConfigurationException : N2CoreException
{
    public ConfigurationException(string message) : base(message)
    {
        ErrorCode = 412;
    }

    public ConfigurationException()
    {
        ErrorCode = 412;
    }

    public ConfigurationException(string message, Exception innerException) : base(message, innerException)
    {
        ErrorCode = 412;
    }
}
