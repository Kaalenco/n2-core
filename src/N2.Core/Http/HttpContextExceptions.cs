namespace N2.Core.Http;

public static class HttpContextExceptions
{
    public static Exception HttpContextAccessorNotFound { get; } = new ArgumentException("Could not locate the Http Context accessor");
    public static Exception HttpContextNotFound { get; } = new ArgumentException("Could not create an http context");
}
