using System.Text.Json;

namespace N2.Core.Extensions;
public static class HttpClientExtensions
{
    public static async Task<JsonDocument> ReadJsonDocument(this HttpClient client, Uri requestUri, CancellationToken cancellationToken)
    {
        Contract.NotNull(client, nameof(client));
        using HttpResponseMessage response = await client.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();
#if NETSTANDARD
        using Stream content = await response.Content.ReadAsStreamAsync();
#else
        using Stream content = await response.Content.ReadAsStreamAsync(cancellationToken);
#endif
        return await JsonDocument.ParseAsync(content, default, cancellationToken);
    }
}
