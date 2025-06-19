using System.IO.Abstractions;

using Kaalenco.Common.SystemAbstractions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using N2.Core;
using N2.Core.Commands;
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

    /// <summary>
    /// Adds the command handlers.
    /// </summary>
    /// <param name="serviceCollection">
    /// The service collection.
    /// </param>
    /// <param name="typeReference">
    /// The type reference.
    /// </param>
    /// <returns>
    /// An IServiceCollection.
    /// </returns>
    public static IServiceCollection AddCommandHandlers(
        this IServiceCollection serviceCollection,
        params Type[] typeReference)
    {
        if (typeReference == null)
        {
            return serviceCollection;
        }

        foreach (Type type in typeReference)
        {
            AddCommandHandlers(serviceCollection, type);
        }
        return serviceCollection;
    }

    /// <summary>
    /// Adds the command handlers.
    /// </summary>
    /// <param name="serviceCollection">
    /// The service collection.
    /// </param>
    /// <param name="typeReference">
    /// The type reference.
    /// </param>
    /// <returns>
    /// An IServiceCollection.
    /// </returns>
    public static IServiceCollection AddCommandHandlers(
        this IServiceCollection serviceCollection,
        Type typeReference)
    {
        if (typeReference == null)
        {
            return serviceCollection;
        }

        System.Reflection.Assembly? assembly = System.Reflection.Assembly.GetAssembly(typeReference);
        if (assembly == null)
        {
            return serviceCollection;
        }

        return AddCommandHandlers(serviceCollection, assembly);
    }

    /// <summary>
    /// Adds the command handlers.
    /// </summary>
    /// <param name="serviceCollection">
    /// The service collection.
    /// </param>
    /// <param name="assembly">
    /// The assembly.
    /// </param>
    /// <returns>
    /// An IServiceCollection.
    /// </returns>
    public static IServiceCollection AddCommandHandlers(this IServiceCollection serviceCollection, System.Reflection.Assembly assembly)
    {
        if (assembly == null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        foreach (Type type in assembly.GetTypes())
        {
            foreach (Type reference in type.GetInterfaces())
            {
                if (reference.IsAssignableTo(typeof(ICommandHandler)) && reference.Name != nameof(ICommandHandler))
                {
                    serviceCollection.AddScoped(reference, type);
                }
            }
        }
        return serviceCollection;
    }

    /// <summary>
    /// Determines whether the current type is assignable to the specified type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="targetType">The target type to check against.</param>
    /// <returns>True if the current type is assignable to the target type; otherwise, false.</returns>
    public static bool IsAssignableTo(this Type type, Type targetType)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        if (targetType == null)
        {
            throw new ArgumentNullException(nameof(targetType));
        }

        // Check direct assignability
        if (targetType.IsAssignableFrom(type))
        {
            return true;
        }

        // Check for generic type definitions
        if (targetType.IsGenericTypeDefinition)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == targetType;
        }

        return false;
    }
}