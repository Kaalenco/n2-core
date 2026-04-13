using Kaalenco.Common.SystemAbstractions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using N2.Core;
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
        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsNested)
            {
                continue;
            }

            foreach (var reference in type.GetInterfaces())
            {
                if (reference.IsAssignableTo(typeof(T)) && reference.Name != nameof(T))
                {
                    if (reference.IsGenericType)
                    {
                        // Register generic type
                        var genericType = reference.GetGenericTypeDefinition();
                        var genericArguments = type.GetGenericArguments();

                        if (genericArguments.Length == 0)
                        {
                            // Register as generic type, if not already registered with the same implementation
                            if (serviceCollection.Any(s => s.ServiceType == reference && s.ImplementationType == type))
                            {
                                continue; // Skip if already registered
                            }
                            serviceCollection.AddScoped(reference, type);
                            continue;
                        }

                        var constructedType = genericType.MakeGenericType(genericArguments);

                        // check if the constructed type is already registered with the same implementation
                        if (serviceCollection.Any(s => s.ServiceType == constructedType && s.ImplementationType == type))
                        {
                            continue; // Skip if already registered
                        }

                        serviceCollection.AddScoped(constructedType, type);
                    }
                    else
                    {
                        // Register non-generic type, if not already registered with the same implementation
                        if (serviceCollection.Any(s => s.ServiceType == reference && s.ImplementationType == type))
                        {
                            continue; // Skip if already registered
                        }
                        serviceCollection.AddScoped(reference, type);
                    }
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