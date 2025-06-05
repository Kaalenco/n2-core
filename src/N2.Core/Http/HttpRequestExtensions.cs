using System.Net;
using System.Text;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace N2.Core.Http;

public static class HttpRequestExtensions
{
    public static HttpResult SuccessResult(this HttpRequest request, SuccessReason reason, string? message = null)
    {
        Contract.NotNull(request, nameof(request));
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
        Contract.NotNull(request, nameof(request));
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
        Contract.NotNull(request, nameof(request));
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
        Contract.NotNull(request, nameof(request));
        request!.Headers.TryGetValue(HttpHeaderType.AuditTag, out StringValues auditTag);
        Uri refUri = new(GetEncodedUrl(request));

        return new HttpResult
        {
            AuditTag = auditTag,
            ReferenceUri = refUri
        };
    }

    private static HttpResult<T> BuildResultFromRequest<T>(HttpRequest? request, T result)
    {
        Contract.NotNull(request, nameof(request));
        request!.Headers.TryGetValue(HttpHeaderType.AuditTag, out StringValues auditTag);
        Uri refUri = new(GetEncodedUrl(request));
        return new HttpResult<T>
        {
            AuditTag = auditTag,
            ReferenceUri = refUri,
            Result = result
        };
    }

    public static string GetEncodedUrl(this HttpRequest request)
    {
        Contract.NotNull(request, nameof(request));
        return BuildAbsolute(request.Scheme, request.Host, request.PathBase, request.Path, request.QueryString);
    }

    public static string BuildAbsolute(
            string scheme,
            HostString host,
            PathString pathBase = new PathString(),
            PathString path = new PathString(),
            QueryString query = new QueryString(),
            FragmentString fragment = new FragmentString())
    {
        Contract.NotNull(scheme, nameof(scheme));

        string combinedPath = (pathBase.HasValue || path.HasValue) ? (pathBase + path).ToString() : "/";

        string encodedHost = host.ToString();
        string encodedQuery = query.ToString();
        string encodedFragment = fragment.ToString();

        // PERF: Calculate string length to allocate correct buffer size for StringBuilder.
        int length = scheme.Length + SchemeDelimiter.Length + encodedHost.Length
            + combinedPath.Length + encodedQuery.Length + encodedFragment.Length;

        return new StringBuilder(length)
            .Append(scheme)
            .Append(SchemeDelimiter)
            .Append(encodedHost)
            .Append(combinedPath)
            .Append(encodedQuery)
            .Append(encodedFragment)
            .ToString();
    }

    public const string SchemeDelimiter = "://";
}
