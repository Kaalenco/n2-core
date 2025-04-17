using System.Net;

using Microsoft.AspNetCore.Http;

namespace N2.Core.Http;

public static class HttpRequestExtensions
{
    public static HttpResult SuccessResult(this HttpRequest request, SuccessReason reason, string? message = null)
    {
        HttpResult result = BuildResultFromRequest(request);
        result.StatusCode = reason switch
        {
            SuccessReason.Success => HttpStatusCode.OK,
            SuccessReason.Removed => HttpStatusCode.NoContent,
            SuccessReason.Created => HttpStatusCode.Created,
            SuccessReason.Modified => HttpStatusCode.Accepted,
            _ => HttpStatusCode.OK
        };
        result.Message = message;
        return result;
    }

    public static HttpResult<T> ValueResult<T>(this HttpRequest request, SuccessReason reason, T value, string? message = null) where T : class
    {
        HttpStatusCode statusCode = reason switch
        {
            SuccessReason.Success => HttpStatusCode.OK,
            SuccessReason.Removed => throw new ArgumentOutOfRangeException(nameof(reason), "Cannot create a value result with reason==Removed"),
            SuccessReason.Created => HttpStatusCode.Created,
            SuccessReason.Modified => HttpStatusCode.Accepted,
            _ => HttpStatusCode.OK
        };
        HttpResult<T> result = BuildResultFromRequest<T>(request, value);
        result.Message = message;
        result.StatusCode = statusCode;
        return result;
    }

    public static HttpResult FailedResult(this HttpRequest request, FailureReason reason, string? message = null)
    {
        HttpResult result = BuildResultFromRequest(request);
        result.StatusCode = reason switch
        {
            FailureReason.Unknown => HttpStatusCode.NotImplemented,
            FailureReason.NotFound => HttpStatusCode.NotFound,
            FailureReason.NotAccepted => HttpStatusCode.NotAcceptable,
            FailureReason.NotModified => HttpStatusCode.NotModified,
            _ => HttpStatusCode.InternalServerError
        };
        result.Message = message;
        return result;
    }

    private static HttpResult BuildResultFromRequest(HttpRequest? request)
    {
        ArgumentNullException.ThrowIfNull(request);
        Microsoft.Extensions.Primitives.StringValues etag = request.Headers.ETag;
        Uri refUri = new(Microsoft.AspNetCore.Http.Extensions.UriHelper.GetEncodedUrl(request));
        return new HttpResult
        {
            Etag = etag,
            ReferenceUri = refUri
        };
    }

    private static HttpResult<T> BuildResultFromRequest<T>(HttpRequest? request, T result)
    {
        ArgumentNullException.ThrowIfNull(request);
        Microsoft.Extensions.Primitives.StringValues etag = request.Headers.ETag;
        Uri refUri = new(Microsoft.AspNetCore.Http.Extensions.UriHelper.GetEncodedUrl(request));
        return new HttpResult<T>
        {
            Etag = etag,
            ReferenceUri = refUri,
            Result = result
        };
    }
}
