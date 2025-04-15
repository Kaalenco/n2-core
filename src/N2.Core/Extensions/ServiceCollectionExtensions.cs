using System.IO.Abstractions;

using Kaalenco.Common.SystemAbstractions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using N2.Core;
using N2.Core.SystemAbstractions;
using N2.Core.Telemetry;

namespace Kaalenco.Common.Extensions;

public static class SystemServiceConfiguration
{
    public static IServiceCollection AddSystemServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IFileSystem>(new FileSystem());
        services.TryAddSingleton<ITimeSystem>(new TimeSystem());
        services.TryAddSingleton<IActivityLoggerFactory>(new ActivityLoggerFactory());
        return services;
    }
}