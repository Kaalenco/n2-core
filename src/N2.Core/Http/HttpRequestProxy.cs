using System.Collections.ObjectModel;
using System.IO.Pipelines;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace N2.Core.Http;

public class HttpRequestProxy : IHttpRequest
{
    private readonly HttpRequest baseRequest;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public HttpRequestProxy(HttpRequest baseRequest)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        Contract.NotNull(baseRequest, nameof(baseRequest));
        this.baseRequest = baseRequest;
    }
    public string? ContentType { get => baseRequest.ContentType; set { baseRequest.ContentType = value; } }
    public long? ContentLength { get => baseRequest.ContentLength; set { baseRequest.ContentLength = value; } }
    public PipeReader BodyReader => PipeReader.Create(baseRequest.Body);
    public Stream Body { get => baseRequest.Body; set { baseRequest.Body = value; } }

    public ReadOnlyDictionary<string, string> Headers
    {
        get => new(baseRequest.Headers.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value.ToString()));
    }

    public string Protocol { get => baseRequest.Protocol; }
    public string QueryString { get => baseRequest.QueryString.ToString(); }
    public string Path { get => baseRequest.Path.ToString(); }
    public string PathBase { get => baseRequest.PathBase.ToString(); }
    public string Host { get => baseRequest.Host.ToString(); }
    public bool IsHttps { get => baseRequest.IsHttps; }
    public string Scheme { get => baseRequest.Scheme; }
    public string Method { get => baseRequest.Method; }
    public IEnumerable<KeyValuePair<string, StringValues>>? Query => baseRequest.Query;
    ReadOnlyDictionary<string, string> IHttpRequest.Headers { get; }

    public Task<string> ReadAsStringAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
}

