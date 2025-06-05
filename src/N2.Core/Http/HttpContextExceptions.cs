using System.IO.Pipelines;

using Microsoft.AspNetCore.Http;

namespace N2.Core.Http;

public static class HttpContextExceptions
{
    public static Exception HttpContextAccessorNotFound { get; } = new ArgumentException("Could not locate the Http Context accessor");
    public static Exception HttpContextNotFound { get; } = new ArgumentException("Could not create an http context");

    public static PipeReader BodyReader(this HttpRequest request)
    {
        Contract.NotNull(request, nameof(request));
        if (request.BodyReader == null)
        {
            throw new InvalidOperationException("The BodyReader is not initialized.");
        }

        request.Body.Position = 0; // Reset the position to the beginning of the stream
        return PipeReader.Create(request.Body);
    }
}
