using System.Net;
using System.Text.Json;

namespace N2.Core.Http;

public class HttpResult : IHttpResult
{
    public HttpStatusCode StatusCode { get; set; }

    public bool IsSuccess()
    {
        return (int)StatusCode <= 300;
    }

    public bool SerializationError { get; set; }
    public string? Message { get; set; }
    public string? Etag { get => auditTag; set => auditTag = value; }
    public string? AuditTag { get => auditTag; set => auditTag = value; }
    public Uri? ReferenceUri { get; set; }

    private static readonly HttpResult okResult = new() { StatusCode = HttpStatusCode.OK };
    private static readonly HttpResult noContentResult = new() { StatusCode = HttpStatusCode.NoContent };
    private static readonly HttpResult notFoundResult = new() { StatusCode = HttpStatusCode.NotFound };
    private static readonly HttpResult notAcceptedResult = new() { StatusCode = HttpStatusCode.NotAcceptable };
    private string? auditTag;

    private static HttpResult internalErrorResult(string message) => new() { StatusCode = HttpStatusCode.InternalServerError, Message = message };

    public static HttpResult Ok() => okResult;

    public static HttpResult NoContent() => noContentResult;

    public static HttpResult NotFound() => notFoundResult;

    public static HttpResult NotAccepted() => notAcceptedResult;

    public static HttpResult Ok(string message) => new() { StatusCode = HttpStatusCode.OK, Message = message };

    public static HttpResult<T> Ok<T>(T result, string? message = null) => new() { StatusCode = HttpStatusCode.OK, Message = message, Result = result };

    public static HttpResult<T> Created<T>(T result, string? message = null) => new() { StatusCode = HttpStatusCode.Created, Message = message, Result = result };

    public static HttpResult<T> Modified<T>(T result, string? message = null) => new() { StatusCode = HttpStatusCode.Accepted, Message = message, Result = result };

    public static HttpResult NoContent(string message) => new() { StatusCode = HttpStatusCode.NoContent, Message = message };

    public static HttpResult NotFound(string message) => new() { StatusCode = HttpStatusCode.NotFound, Message = message };

    public static HttpResult NotAccepted(string message) => new() { StatusCode = HttpStatusCode.NotAcceptable, Message = message };

    public static HttpResult Exception(Exception e) => internalErrorResult(e?.Message ?? string.Empty);
}

public class HttpResult<T> : HttpResult, IHttpResult<T>
{
    public T Result { get; set; } = default!;

    public Task WriteJsonResponse(IHttpContext context, JsonSerializerOptions options, CancellationToken token)
    {
        Contract.NotNull(context, nameof(context));

        IHttpResponse response = context.Response;
        response.ClearHeaders();
        response.SetHeader(HttpHeaderType.ContentType, "application/json");
        response.SetHeader(HttpHeaderType.Type, typeof(T).FullName!);
        if (!string.IsNullOrEmpty(Message))
        {
            response.SetHeader(HttpHeaderType.Message, Message!);
        }

        if (!string.IsNullOrEmpty(AuditTag))
        {
            response.SetHeader(HttpHeaderType.AuditTag, AuditTag!);
        }

        if (ReferenceUri != null)
        {
            response.SetHeader(HttpHeaderType.ReferenceUri, ReferenceUri.ToString());
        }

        return response.WriteAsJsonAsync(Result, options, token);
    }
}

public static class HttpHeaderType
{
    public const string ContentType = "Content-Type";
    public const string AuditTag = "X-AuditTag";
    public const string ReferenceUri = "X-ReferenceUri";
    public const string Message = "X-Message";
    public const string Type = "X-Type";
}