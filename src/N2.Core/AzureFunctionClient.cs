using System.Text;
using System.Text.Json;

namespace N2.Core;

public class AzureFunctionClient : IDisposable, IAzureFunctionClient
{
    private readonly HttpClient httpClient = new();
    private readonly IActivityLogger activityLogger;
    private readonly IDefaultValueService defaultValueService;
    private readonly string apiKey;
    private readonly Uri basePath;
    private bool disposedValue;

    public AzureFunctionClient(
        IActivityLogger activityLogger,
        IDefaultValueService defaultValueService,
        Uri basePath,
        string apiKey)
    {
        this.basePath = basePath;
        this.defaultValueService = defaultValueService;
        this.apiKey = apiKey;
        this.activityLogger = activityLogger;
    }

    public async Task<TA?> CallAsync<TQ, TA>(string functionPath, TQ request, CancellationToken cancellationToken)
    {
        JsonSerializerOptions options = (JsonSerializerOptions)defaultValueService.JsonSerializerOptions;
        string content = JsonSerializer.Serialize(request, options);
        Uri url = new(basePath, $"/api/{functionPath}");
        using StringContent httpRequest = new(content, Encoding.UTF8, "application/json");
        httpRequest.Headers.Add("x-functions-key", apiKey);

        HttpResponseMessage response = await httpClient.PostAsync(url, httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            activityLogger.Tag("Error", $"Failed to get response from {url}. Status code: {response.StatusCode}");
            return default;
        }

        if (response.Content == null)
        {
            activityLogger.Tag("Error", $"No content in response from {url}");
            return default;
        }

#if NETSTANDARD
        string result = await response.Content.ReadAsStringAsync();
#else
        string result = await response.Content.ReadAsStringAsync(cancellationToken);
#endif
        if (string.IsNullOrEmpty(content))
        {
            activityLogger.Tag("Warning", $"Empty response from {url}");
            return default;
        }

        if (typeof(TA) == typeof(string))
        {
            return (TA)(object)result;
        }

        return JsonSerializer.Deserialize<TA>(content, (JsonSerializerOptions)defaultValueService.JsonSerializerOptions);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                httpClient.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}