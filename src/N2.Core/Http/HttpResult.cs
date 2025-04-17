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
    public string? Etag { get; set; }
    public Uri? ReferenceUri { get; set; }

    private static readonly HttpResult okResult = new() { StatusCode = HttpStatusCode.OK };
    private static readonly HttpResult noContentResult = new() { StatusCode = HttpStatusCode.NoContent };
    private static readonly HttpResult notFoundResult = new() { StatusCode = HttpStatusCode.NotFound };
    private static readonly HttpResult notAcceptedResult = new() { StatusCode = HttpStatusCode.NotAcceptable };
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
        ArgumentNullException.ThrowIfNull(context);

        IHttpResponse response = context.Response;
        response.ClearHeaders();
        response.SetHeader("Content-Type", "application/json");
        response.SetHeader("X-Type", typeof(T).FullName!);
        if (!string.IsNullOrEmpty(Message))
        {
            response.SetHeader("X-Message", Message);
        }

        if (!string.IsNullOrEmpty(Etag))
        {
            response.SetHeader("Etag", Etag);
        }

        if (ReferenceUri != null)
        {
            response.SetHeader("X-ReferenceUri", ReferenceUri.ToString());
        }

        return response.WriteAsJsonAsync(Result, options, token);
    }
}