using System.Globalization;
using System.Resources;

using Microsoft.Extensions.Caching.Memory;

namespace N2.Core;

public class LocalizedTextService : ITranslator
{
    public const int DefaultCacheTimeInMinutes = 10;
    public int CacheTimeInMinutes { get; set; } = DefaultCacheTimeInMinutes;

    public CultureInfo CurrentCulture
    {
        get => currentCulture;
        set
        {
            if (value == null)
            {
                currentCulture = CultureInfo.CurrentUICulture;
                Language = CultureInfo.CurrentUICulture.Name;
            }
            else
            {
                currentCulture = value;
                Language = value.Name;
            }
        }
    }

    public string Language { get; private set; }

    private readonly ResourceManager _resourceManager;
    private readonly IMemoryCache _cache;
    private CultureInfo currentCulture;

    public LocalizedTextService(ResourceManager resourceManager, IMemoryCache memoryCache)
    {
        currentCulture = CultureInfo.CurrentUICulture;
        Language = CurrentCulture.Name;
        _resourceManager = resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));
        _cache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public string GT(string key)
    {
        return GetTextFromResource(null, key, CurrentCulture);
    }

    public string GT(string pageContext, string key)
    {
        return GetTextFromResource(pageContext, key, CurrentCulture);
    }

    public string Translate(string language, string key)
    {
        CultureInfo culture = string.IsNullOrEmpty(language) ? CurrentCulture : new CultureInfo(language);
        return GetTextFromResource(null, key, culture)!;
    }

    public string Translate(string language, string pageContext, string key)
    {
        CultureInfo culture = string.IsNullOrEmpty(language) ? CurrentCulture : new CultureInfo(language);

        return GetTextFromResource(pageContext, key, culture);
    }

    private string GetTextFromResource(string? pageContext, string key, CultureInfo culture)
    {
        string? result = null;
        if (!string.IsNullOrEmpty(pageContext))
        {
            // Logic to handle pageContext if needed
            string contextKey = $"{pageContext}:{key}";
            result = GetTextFromResource(contextKey, culture);
            if (result != null)
            {
                return result;
            }
        }
        result = GetTextFromResource(key, CurrentCulture);
        return result ?? $"[Missing: {key}]";
    }

    private string? GetTextFromResource(string key, CultureInfo culture)
    {
        string cacheKey = $"{culture.Name}:{key}";

        // Try to get the value from the cache
        if (_cache.TryGetValue(cacheKey, out string? cachedValue))
        {
            return cachedValue!;
        }

        // If not in cache, load from the resource manager
        string? value = _resourceManager.GetString(key, culture);

        // Store the value in the cache
        if (value == null)
        {
            return null;
        }

        _cache.Set(cacheKey, value, TimeSpan.FromMinutes(CacheTimeInMinutes)); // Cache for 10 minutes
        return value;
    }
}