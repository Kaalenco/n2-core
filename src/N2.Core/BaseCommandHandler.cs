using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using N2.Core.Commands;

namespace N2.Core;

#pragma warning disable CA1031 // Do not catch general exception types


/// <summary>
/// The base command handler.
/// </summary>
public abstract class BaseCommandHandler<TQ, TA> : ICommandHandler<TQ, TA>
     where TQ : class, ICommandRequest
     where TA : class, ICommandResponse, new()
{
    /// <summary>
    /// The default timeout in milliseconds.
    /// </summary>
    public const int DefaultTimeoutInMilliseconds = 200;
    private readonly TA response = new();

    private static readonly JsonSerializerOptions options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// The long timeout of 10 minutes used when the timeout is set to 0 or a negative value.
    /// </summary>
    public const int LongTimeout = 60000 * 10;

    private TQ _request = null!;
    private TA _result = null!;

    /// <summary>
    /// Gets a value indicating whether a processing error occured.
    /// </summary>
    public bool ProcessingError { get; protected set; }

    /// <summary>
    /// Gets or sets the timeout in milli seconds.
    /// </summary>
    public int TimeoutInMilliSeconds { get; set; } = DefaultTimeoutInMilliseconds;

    /// <summary>
    /// Gets the logger.
    /// </summary>
    protected ILogger Logger { get; private set; }

    /// <summary>
    /// Indication that the commandhandler is currently active and should not be disposed or removed from the conductor.
    /// Use this to indicate that the commandhandler is currently processing requests or waiting for responses.
    /// </summary>
    public virtual bool IsActive => false;

    /// <summary>
    /// Indication that the commandhandler is enabled and can handle requests.
    /// </summary>
    public virtual bool IsEnabled => true;

    private readonly IRuntimeValidator<TQ> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseCommandHandler{TQ, TA}"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="validator">The validator.</param>
    protected BaseCommandHandler(ILogger logger, IRuntimeValidator<TQ> validator)
    {
        Contract.NotNull(logger, nameof(logger));

        Logger = logger;
        _validator = validator;
    }

    /// <summary>
    /// Validates the request.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>A ResponseStatus.</returns>
    protected ResponseStatus Validate(TQ request)
    {
        Contract.NotNull(request, nameof(request));
        try
        {
            _validator?.Validate(request);
            return ResponseStatus.Accepted;
        }
        catch (Exception ex)
        {
            Dictionary<string, object?> data = new()
            {
                { "Request", request },
                { "RequestType", request.GetType().Name }
            };
            LogValidationError(Logger, JsonSerializer.Serialize(data), ex);
            return ResponseStatus.NotAcceptable;
        }
    }

    /// <summary>
    /// LoggerMessage delegate for logging Validation errors.
    /// </summary>
    private static readonly Action<ILogger, string, Exception?> LogValidationError =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(1, nameof(Validate)),
            "Validation failed for request: {Request}");

    /// <summary>
    /// LoggerMessage delegate for logging timeout errors.
    /// </summary>
    private static readonly Action<ILogger, string, Exception?> LogExecutionError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(2, nameof(WaitForAsync)),
            "Execution failed for request: {Request}");


    /// <summary>
    /// Accepts the request for processing.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <exception cref="NotImplementedException"></exception>
    /// <returns>A ResponseStatus.</returns>
    public virtual ResponseStatus ExecuteCommand(TQ request)
    {
        _request = request;
        return Invoke();
    }

    ResponseStatus ICommandHandler.Invoke(ICommandRequest request)
    {
        if (request is TQ commandRequest)
        {
            _request = commandRequest;
            return Invoke();
        }
        return ResponseStatus.NotAcceptable;
    }


    /// <summary>
    /// Handles the request.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    /// <returns>A ResponseStatus.</returns>
    public ResponseStatus Invoke()
    {
        if (_request == null)
        {
            return ResponseStatus.NoContent;
        }

        _result = HandleRequest(_request);
        return _result.Status;
    }

    /// <summary>
    /// Invokes the commandhandler, waiting for the result.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>A TA.</returns>
    [SuppressMessage("Design",
        "CA1062:Validate arguments of public methods",
        Justification = "Validate checks for null values")]
    private TA HandleRequest(TQ request)
    {
        ResponseStatus status = Validate(request);
        if (!Guid.TryParse(request.Handle, out Guid handle))
        {
            handle = Guid.NewGuid();
        }

        if (status != ResponseStatus.Accepted)
        {
            return (TA)response.CreateNew(status, "Validation failed for the request", handle);
        }

        int timeoutMs = TimeoutInMilliSeconds > 0 ? TimeoutInMilliSeconds : LongTimeout;
        Task<TA> task = HandleRequestAsync(request);

        if (task.Wait(timeoutMs))
        {
            _result = task.Result;
            return task.Result;
        }

        // Note: the handler task continues running to completion on the ThreadPool.
        // Task.Wait only stops waiting — it does not cancel execution. Any side effects
        // (DB writes, HTTP calls, etc.) will still occur. True cancellation requires
        // passing a CancellationToken into HandleRequestAsync.
        return (TA)response.CreateNew(
            ResponseStatus.TimeOut,
            $"Could not complete request, timeout occured after {timeoutMs} milliseconds.",
            handle);
    }

    /// <summary>
    /// Return the response message.
    /// </summary>
    /// <param name="peek">If true, peek.</param>
    /// <returns>A response object.</returns>
    public virtual TA Response(bool peek)
    {
        return _result;
    }

    /// <summary>
    /// Invokes the command async.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns><![CDATA[Task<TA>]]></returns>
    public abstract Task<TA> HandleRequestAsync(TQ request);

    /// <summary>
    /// Waits the for async.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>A Task.</returns>
    [SuppressMessage("Design",
        "CA1062:Validate arguments of public methods",
        Justification = "Validate checks for null values")]
    public Task<TA> WaitForAsync(TQ request)
    {
        ResponseStatus status = Validate(request);
        if (!Guid.TryParse(request.Handle, out Guid handle))
        {
            handle = Guid.NewGuid();
        }

        if (status != ResponseStatus.Accepted)
        {
            _result = (TA)response.CreateNew(
                status,
                $"Command not accepted: {status}\n {JsonSerializer.Serialize(request, options)}",
                handle);
            return Task.FromResult(_result);
        }
        try
        {
            return HandleRequestAsync(request);
        }
        catch (Exception ex)
        {
            LogExecutionError(Logger, JsonSerializer.Serialize(request, options), ex);
            _result = (TA)response.CreateNew(ResponseStatus.ServerError, ex.Message, handle);
            return Task.FromResult(_result);
        }

    }

    /// <summary>
    /// The Canhandle method should not validate the content, just return true
    /// if the command can be handled or not.
    /// </summary>
    /// <param name="command">A command instance.</param>
    /// <returns>true if the command can be handled by the command handler.</returns>
    public abstract bool CanHandle(ICommandRequest command);
}

#pragma warning restore CA1031 // Do not catch general exception types
