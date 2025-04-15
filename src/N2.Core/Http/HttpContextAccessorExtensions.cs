using Microsoft.AspNetCore.Http;

namespace N2.Core.Http;

public static class HttpContextAccessorExtensions
{
    public static T FromQueryString<T>(this IHttpContextAccessor httpContextAccessor) where T : class, new()
    {
        var accessor = httpContextAccessor ?? throw HttpContextExceptions.HttpContextAccessorNotFound;
        var result = new T();
        var context = accessor.HttpContext;
        if (context == null || context.Request.Query == null)
        {
            return result;
        }
        var query = context.Request.Query;
        var properties = typeof(T).GetProperties();
        foreach (var (property, value) in from property in properties
                                          where query.ContainsKey(property.Name)
                                          let value = query[property.Name]
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
}