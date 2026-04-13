using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using N2.Core.Commands;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace N2.Core.PubSub;

/// <summary>
/// Provides an in-memory implementation of the change notification service for tracking and notifying listeners about
/// changes to items.
/// </summary>
/// <remarks>This service is intended for scenarios where change notifications are managed within the
/// application's memory space. It is not suitable for distributed or persistent notification requirements. Use this
/// class to register listeners and notify them when tracked items are modified. It should be registered as a singleton
/// within the application's dependency injection container.</remarks>
public class InMemoryChangeService : INotifyChangeService
{
    private readonly ConcurrentDictionary<string, INotifyChangeListener[]> _listeners = new();
    private readonly ILogger _logger;

    public InMemoryChangeService(ILogger<InMemoryChangeService> logger)
    {
        // No initialization required for in-memory implementation
        _logger = logger;
        _logger.ServiceInitialized();
    }

    public void AddSubscription(INotifyChangeListener listener) {

        if (listener == null)
        {
            return;
        }
            var typeName = listener.GetType().FullName ?? "UnknownType";
#pragma warning disable CA1031 // Do not catch general exception types
        try
        {

            var listeners = _listeners.GetOrAdd(typeName, _ => []);
            var newList = listeners.ToList();
            newList.Add(listener);
            _listeners[typeName] = [.. newList];
            _logger.SubscriptionInitialized(typeName);
        }
        catch (Exception ex)
        {
            _logger.SubscriptionFailed(ex, typeName);
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }

    public void ItemModified<T>(TrackingId trackingId, T item) where T : IItemChanged
    {
        var typeName = typeof(T).FullName ?? "UnknownType";
        if (_listeners.TryGetValue(typeName, out var listeners))
        {
            foreach (var listener in listeners)
            {
#pragma warning disable CA1031 // Do not catch general exception types
                try
                {
                    listener.OnItemModified(item);
                }
                catch (Exception ex)
                {
                    _logger.ItemModifiedFailure( typeName, ex);
                }
#pragma warning restore CA1031 // Do not catch general exception types
            }
        }
    }

    public void RemoveSubscription(INotifyChangeListener listener) {
        if (listener == null) { return; }
        var typeName = listener.GetType().FullName ?? "UnknownType";
        foreach (var kvp in _listeners)
        {
            var listeners = kvp.Value;
            if (listeners.Contains(listener))
            {
                var newList = listeners.ToList();
                newList.Remove(listener);
                _listeners[kvp.Key] = [.. newList];
            }
        }
    }

    public static void Register(IServiceCollection services)
    {
        if(services == null) throw new ArgumentNullException(nameof(services));

        services.AddSingleton<INotifyChangeService, InMemoryChangeService>();
    }
}

internal static partial class InMemoryChangeServiceLoggerExtensions
{
    public static void ServiceInitialized(this ILogger logger)
    {
        LogServiceInitialized(logger);
    }

    public static void SubscriptionFailed(this ILogger logger, Exception ex, string typeName)
    {
        LogSubscriptionFailed(logger, typeName, ex);
    }

    public static void SubscriptionInitialized(this ILogger logger, string typeName)
    {
        LogSubscriptionInitialized(logger, typeName);
    }

    public static void ItemModifiedFailure(this ILogger logger, string typeName, Exception ex)
    {
        LogItemModifiedFailure(logger, typeName, ex);
    }

    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "InMemoryChangeService initialized.")]
    static partial void LogServiceInitialized(ILogger logger);

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "New Subscription on: {TypeName}")]
    static partial void LogSubscriptionInitialized(ILogger logger, string typeName);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Failed to notify listener of type {TypeName} about item modification.")]
    static partial void LogItemModifiedFailure(ILogger logger, string typeName, Exception ex);

    [LoggerMessage(EventId = 500, Level = LogLevel.Error, Message = "Failed to add subscription for listener of type {TypeName}")]
    static partial void LogSubscriptionFailed(ILogger logger, string typeName, Exception ex);
}


