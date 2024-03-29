namespace Dpi.Repository.Exceptions;

public class ElementNotFoundException : N2CoreException
{
    public string ElementName { get; }
    public ElementNotFoundException(string elementName) : base("The item could not be found.")
    {
        ElementName = elementName;
        ErrorCode = 404;
    }

    public ElementNotFoundException()
    {
        ElementName = string.Empty;
        ErrorCode = 404;
    }

    public ElementNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
        ElementName = string.Empty;
        ErrorCode = 404;
    }
}
