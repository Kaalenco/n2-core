using System.IO.Pipelines;

using Microsoft.AspNetCore.Http;

namespace N2.Core.Http;

public static class HttpContextExtensions
{
    public static PipeReader RequestBody(this HttpRequest request)
    {
        Contract.NotNull(request, nameof(request));
        if (request.Body == null)
        {
            throw new InvalidOperationException("The BodyReader is not initialized.");
        }

        request.Body.Position = 0; // Reset the position to the beginning of the stream
        return PipeReader.Create(request.Body);
    }
}
