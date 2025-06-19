using System.Collections.Concurrent;
using System.Text.Json;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NCore.Common.Extensions;


namespace N2.Core.Commands;

public class Conductor : IConductor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Conductor> _logger;
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<Guid, Action<ICommandResponse>> _callBack = new();

    public Conductor(
        IServiceProvider serviceProvider,
        IMemoryCache cache,
        ILogger<Conductor> logger)
    {
        Contract.NotNull(serviceProvider, nameof(serviceProvider));
        Contract.NotNull(cache, nameof(cache));
        Contract.NotNull(logger, nameof(logger));

        _serviceProvider = serviceProvider;
        _logger = logger;
        _cache = cache;
    }

    public bool IsConfigured => _serviceProvider != null;
    public int CallbackHandlerCount => _callBack.Count;
    private int _threadTimeout = 10;

    public int ThreadTimeOut
    {
        get => _threadTimeout; set
        {
            if (value < 10)
            {
                _threadTimeout = 10;
            }
            else
            {
                _threadTimeout = value;
            }
        }
    }

    public bool IsActive => _activeCommandThreads > 0 || Invoking > 0;

    public int Invoking { get; private set; }

    private static int _activeCommandThreads;
    private static readonly Semaphore _pool = new(0, 20, "ConductorThreads");

    public void CallBack<TResponse>(TResponse result) where TResponse : ICommandResponse
    {
        if (result == null)
        {
            return;
        }

        TrackingId handle = result.Handle ?? Guid.NewGuid();
        _logger.TrackingInfo(handle, $"Check for callback for {result}");
        if (_callBack.TryGetValue(handle, out Action<ICommandResponse>? func))
        {
            if (func != null)
            {
#pragma warning disable CA1031 // Do not catch general exception types
                try
                {
                    func.Invoke(result);
                }
                catch (Exception e)
                {
                    _logger.CallbackInvokeFailed(
                        handle,
                        result.GetType().Name,
                        JsonSerializer.Serialize(result),
                        e);
                }
#pragma warning restore CA1031 // Do not catch general exception types
            }
            else
            {
                _logger.NoCallbackRegistered(handle, $"Empty callback for {typeof(TResponse)}");
            }
        }
        else
        {
            _logger.NoCallbackRegistered(handle, $"No callback for {typeof(TResponse)}");
        }
    }

    public ResponseStatus Invoke<TCommand>(TCommand command) where TCommand : ICommandRequest
    {
        if (_serviceProvider == null || command == null)
        {
            return ResponseStatus.BadRequest;
        }

        if (!Guid.TryParse(command.Handle, out Guid handle))
        {
            handle = Guid.NewGuid();
        }

        Invoking++;
        ICommandHandler<TCommand>[] handlers = _serviceProvider.GetServices<ICommandHandler<TCommand>>().ToArray();
        if (handlers == null)
        {
            _logger.CommandHandlerNotFound(
                handle,
                command.GetType().Name,
                JsonSerializer.Serialize(command));
            return ResponseStatus.NotAcceptable;
        }

        if (handlers.Length == 0)
        {
            _logger.TrackingInfo(handle, $"No handlers for {command}");
        }
        else
        {
            _logger.TrackingInfo(handle, $"Handling command {command}");
            foreach (ICommandHandler<TCommand> handler in handlers)
            {
                Thread t = new(() =>
                {
                    _logger.TrackingInfo(handle, $"Start invoke for {handler.GetType()}");
                    if (_pool.WaitOne(TimeSpan.FromMilliseconds(ThreadTimeOut)))
                    {
                        _activeCommandThreads++;
#pragma warning disable CA1031 // Do not catch general exception types
                        try
                        {
                            handler.Invoke(command);
                        }
                        catch (Exception e)
                        {
                            _logger.InvokeCommandFailed(
                                handle,
                                command.GetType().Name,
                                JsonSerializer.Serialize(command),
                                e);
                        }
#pragma warning restore CA1031 // Do not catch general exception types
                        _activeCommandThreads--;
                        _pool.Release();
                    }
                    else
                    {
                        _logger.ThreadTimeout(handle, $"Could not start {handler.GetType()}");
                    }
                }
                );
                t.Start();
            }
        }
        Invoking--;
        return ResponseStatus.Accepted;
    }

    public bool IsCommandAvailable<TCommand>() where TCommand : ICommandRequest
    {
        if (_serviceProvider == null)
        {
            return false;
        }

        return _serviceProvider.GetService<ICommandHandler<TCommand>>() != null;
    }

    public ICallback RegisterCallback<TResponse>(Action<ICommandResponse> value) where TResponse : ICommandResponse
    {
        Guid handle = Guid.NewGuid();
        _callBack.TryAdd(handle, value);
        return new CallbackRegistration(handle, _callBack);
    }
}

public class CallbackRegistration : IDisposable, ICallback
{
    private readonly Guid _handle;
    private readonly ConcurrentDictionary<Guid, Action<ICommandResponse>> _registrations;
    private bool disposedValue;
    public Guid? Handle => _handle;

    public CallbackRegistration(Guid handle, ConcurrentDictionary<Guid, Action<ICommandResponse>> registrations)
    {
        _registrations = registrations;
        _handle = handle;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _registrations.TryRemove(_handle, out _);
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}