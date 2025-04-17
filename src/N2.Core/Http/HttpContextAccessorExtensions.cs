using Microsoft.Extensions.Primitives;

namespace N2.Core.Http;

public static class HttpContextAccessorExtensions
{
    public static T FromQueryString<T>(this IHttpContextAccessor httpContextAccessor) where T : class, new()
    {
        IHttpContextAccessor accessor = httpContextAccessor ?? throw HttpContextExceptions.HttpContextAccessorNotFound;
        T result = new();
        IHttpContext? context = accessor.HttpContext;
        if (context == null || context.Request.Query == null)
        {
            return result;
        }
        IEnumerable<KeyValuePair<string, StringValues>> query = context.Request.Query;
        System.Reflection.PropertyInfo[] properties = typeof(T).GetProperties();
        foreach ((System.Reflection.PropertyInfo property, Microsoft.Extensions.Primitives.StringValues value)
            in from property in properties
               where query.ContainsKey(property.Name)
               let value = query.GetValue(property.Name)
               where value.Count == 1
               select (property, value))
        {
            property.SetValue(result, value[0]);
        }
        return result;
    }

    public static IHttpContext HttpContextProxy(this IHttpContextAccessor httpContextAccessor)
    {
        return new HttpContextProxy(httpContextAccessor);
    }

    private static StringValues GetValue(this IEnumerable<KeyValuePair<string, StringValues>> query, string key)
    {
        foreach (KeyValuePair<string, StringValues> kvp in query)
        {
            if (string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Value;
            }
        }
        return default;
    }

    private static bool ContainsKey(this IEnumerable<KeyValuePair<string, StringValues>> query, string key)
    {
        foreach (KeyValuePair<string, StringValues> kvp in query)
        {
            if (string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
}