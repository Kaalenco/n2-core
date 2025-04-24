using System.Globalization;
using System.Resources;

using Microsoft.Extensions.Caching.Memory;

namespace N2.Core;

public class LocalizedTextService : ITextService
{
    public const int DefaultCacheTimeInMinutes = 10;
    public int CacheTimeInMinutes { get; set; } = DefaultCacheTimeInMinutes;
    public CultureInfo CurrentCulture { get; set; }
    private readonly ResourceManager _resourceManager;
    private readonly IMemoryCache _cache;

    public LocalizedTextService(ResourceManager resourceManager, IMemoryCache memoryCache)
    {
        CurrentCulture = CultureInfo.CurrentUICulture;
        _resourceManager = resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));
        _cache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public string GetGlobalText(string key)
    {
        return GetTextFromResource(key, CurrentCulture);
    }

    public string GetText(string pageContext, string key)
    {
        // Optionally, you can use the pageContext to determine a specific resource file or namespace.
        return GetTextFromResource(key, CurrentCulture);
    }

    private string GetTextFromResource(string key, CultureInfo culture)
    {
        string cacheKey = $"{culture.Name}:{key}";

        // Try to get the value from the cache
        if (_cache.TryGetValue(cacheKey, out string? cachedValue))
        {
            return cachedValue!;
        }

        // If not in cache, load from the resource manager
        string? value = _resourceManager.GetString(key, culture) ?? $"[Missing: {key}]";

        // Store the value in the cache
        _cache.Set(cacheKey, value, TimeSpan.FromMinutes(CacheTimeInMinutes)); // Cache for 10 minutes

        return value;
    }
}
