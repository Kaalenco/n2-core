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
        ArgumentNullException.ThrowIfNull(request);
        try
        {
            if (_validator != null)
            {
                _validator.Validate(request);
            }
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
    public virtual ResponseStatus Accept(TQ request)
    {
        _request = request;
        return ResponseStatus.Accepted;
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
    public TA HandleRequest(TQ request)
    {
        ResponseStatus status = Validate(request);
        if (status != ResponseStatus.Accepted)
        {
            return (TA)response.CreateNew(status, "Validation failed for the request", request?.Handle);
        }

        Task<TA> task = HandleRequestAsync(request);
        if (task.IsCompleted)
        {
            // Wait for the result to be available.
            task.Wait();
            return task.Result;
        }

        Task timeout = new(() => { Thread.Sleep(TimeoutInMilliSeconds > 0 ? TimeoutInMilliSeconds : LongTimeout); });

        task.Start();
        timeout.Start();
        int finished = Task.WaitAny(new[] { task, timeout });
        if (task.IsCompleted)
        {
            _result = task.Result;
            return task.Result;
        }

        return (TA)response.CreateNew(
            ResponseStatus.TimeOut,
            $"Could not complete request, timeout occured after {TimeoutInMilliSeconds} milliseconds.",
            request?.Handle);
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
    public Task<TA> WaitForAsync(TQ request)
    {
        ResponseStatus status = Validate(request);
        if (status != ResponseStatus.Accepted)
        {
            _result = (TA)response.CreateNew(
                status,
                $"Command not accepted: {status}\n {JsonSerializer.Serialize(request, options)}",
                request?.Handle);
            return Task.FromResult(_result);
        }
        try
        {
            return HandleRequestAsync(request);
        }
        catch (Exception ex)
        {
            LogExecutionError(Logger, JsonSerializer.Serialize(request, options), ex);
            _result = (TA)response.CreateNew(ResponseStatus.ServerError, ex.Message, request?.Handle);
            return Task.FromResult(_result);
        }

    }
}

#pragma warning restore CA1031 // Do not catch general exception types
