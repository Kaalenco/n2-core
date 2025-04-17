using System.Text.Json;

namespace N2.Core.Extensions;
public static class HttpClientExtensions
{
    public static async Task<JsonDocument> ReadJsonDocumentAsync(this HttpClient client, Uri requestUri)
    {
        ArgumentNullException.ThrowIfNull(client);
        using HttpResponseMessage response = await client.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();
        return await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
    }
}
