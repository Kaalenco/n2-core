using System.IO.Pipelines;
using System.Text.Json;

using Microsoft.AspNetCore.Http;

namespace N2.Core.Http;

public class HttpResponseProxy : IHttpResponse
{
    private readonly HttpResponse baseResponse;
    public HttpResponseProxy(HttpResponse baseResponse)
    {
        Contract.NotNull(baseResponse, nameof(baseResponse));
        this.baseResponse = baseResponse;
    }

    public string? ContentType { get => baseResponse.ContentType; set { baseResponse.ContentType = value; } }
    public long? ContentLength { get => baseResponse.ContentLength; set { baseResponse.ContentLength = value; } }
    public PipeWriter BodyWriter => PipeWriter.Create(baseResponse.Body);
    public Stream Body { get => baseResponse.Body; set { baseResponse.Body = value; } }
    public Dictionary<string, string> Headers =>
        new(baseResponse.Headers.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value.ToString()));
    public int StatusCode { get => baseResponse.StatusCode; set { baseResponse.StatusCode = value; } }

    public void Clear() => baseResponse.Body.SetLength(0);

    public void ClearHeaders()
    {
        baseResponse.Headers.Clear();
    }
    public void SetHeader(string key, string values)
    {
        baseResponse.Headers[key] = values;
    }

    public void SetHeader(string name, IEnumerable<string> values)
    {
        baseResponse.Headers[name] = string.Join(",", values);
    }

    public Task WriteAsJsonAsync<T>(T content, JsonSerializerOptions options, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public Task WriteAsync(string content, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task WriteAsync(byte[] content, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}