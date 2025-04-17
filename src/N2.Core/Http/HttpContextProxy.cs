using System.Security.Principal;

using Microsoft.AspNetCore.Http;

namespace N2.Core.Http;

public class HttpContextProxy : IHttpContext
{
    private readonly HttpContext? baseContext;

    public HttpContextProxy(IHttpContextAccessor httpContextAccessor)
    {
        IHttpContextAccessor accessor = httpContextAccessor ?? throw HttpContextExceptions.HttpContextAccessorNotFound;
        baseContext = accessor.HttpContext as HttpContext ?? throw HttpContextExceptions.HttpContextNotFound;
    }

    public HttpContextProxy(HttpContext httpContext)
    {
        baseContext = httpContext;
    }

    public IPrincipal User => baseContext!.User;

    public IHttpResponse Response => new HttpResponseProxy(baseContext!.Response);
    public IHttpRequest Request => new HttpRequestProxy(baseContext!.Request);
}
