using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using N2.Core.Commands;
using N2.Core.SystemAbstractions;
using N2.Core.Telemetry;

using System.IO.Abstractions;
using System.Reflection;

namespace N2.Core.Extensions;

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
    /// Adds the command validators.
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
    public static IServiceCollection AddValidators(
        this IServiceCollection serviceCollection,
        Type typeReference)
    {
        Contract.NotNull(typeReference, nameof(typeReference));
        var assembly = Assembly.GetAssembly(typeReference);
        if (assembly == null)
        {
            return serviceCollection;
        }

        return AddInterfacesOfType<IRuntimeValidator>(serviceCollection, assembly);
    }

    /// <summary>
    /// Adds the command validators.
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
    public static IServiceCollection AddValidators(
        this IServiceCollection serviceCollection,
        Assembly assembly)
    {
        Contract.NotNull(assembly, nameof(assembly));
        return AddInterfacesOfType<IRuntimeValidator>(serviceCollection, assembly);
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
    /// <param name="assembly">
    /// The assembly.
    /// </param>
    /// <returns>
    /// An IServiceCollection.
    /// </returns>
    public static IServiceCollection AddCommandHandlers(
        this IServiceCollection serviceCollection,
        Assembly assembly)
    {
        Contract.NotNull(assembly, nameof(assembly));
        return AddInterfacesOfType<ICommandHandler>(serviceCollection, assembly);
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
        Contract.NotNull(typeReference, nameof(typeReference));
        var assembly = Assembly.GetAssembly(typeReference);
        if (assembly == null)
        {
            return serviceCollection;
        }

        return AddInterfacesOfType<ICommandHandler>(serviceCollection, assembly);
    }

    /// <summary>
    /// Adds the classes to the servicecollection with interfaces of type T.
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
    public static IServiceCollection AddInterfacesOfType<T>(
        this IServiceCollection serviceCollection,
        Assembly assembly)
    {
        Contract.NotNull(assembly, nameof(assembly));

        var candidates = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsNested)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsAssignableTo(typeof(T)) && i.Name != nameof(T))
                .Select(i => (ImplementationType: t, ServiceType: ResolveServiceType(i, t))));

        foreach (var (implementationType, serviceType) in candidates)
        {
            if (!serviceCollection.Any(s => s.ServiceType == serviceType && s.ImplementationType == implementationType))
            {
                serviceCollection.AddScoped(serviceType, implementationType);
            }
        }

        return serviceCollection;
    }

    private static Type ResolveServiceType(Type interfaceType, Type implementationType)
    {
        if (!interfaceType.IsGenericType)
            return interfaceType;

        var genericArgs = implementationType.GetGenericArguments();
        return genericArgs.Length == 0
            ? interfaceType
            : interfaceType
                .GetGenericTypeDefinition()
                .MakeGenericType(genericArgs);
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