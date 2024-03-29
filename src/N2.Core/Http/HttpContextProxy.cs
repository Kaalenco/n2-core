using Microsoft.AspNetCore.Http;
using System.Security.Principal;

namespace N2.Core.Http;

public class HttpContextProxy : IHttpContext
{
    private readonly HttpContext baseContext;

    public HttpContextProxy(IHttpContextAccessor httpContextAccessor)
    {
        var accessor = httpContextAccessor ?? throw HttpContextExceptions.HttpContextAccessorNotFound;
        baseContext = accessor.HttpContext ?? throw HttpContextExceptions.HttpContextNotFound;
    }

    public IPrincipal User => baseContext.User;
}
