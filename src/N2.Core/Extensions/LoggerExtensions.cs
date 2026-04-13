using Microsoft.Extensions.Logging;

using N2.Core.Commands;

namespace N2.Core.Extensions;

public static class LoggerExtensions
{
    public static void TrackingInfo(this ILogger logger, TrackingId Handle, string Message, Exception? e = null)
    {
        trackingInfo(logger, Handle, Message, e);
    }

    public static void ThreadTimeout(this ILogger logger, TrackingId Handle, string Message, Exception? e = null)
    {
        threadTimeout(logger, Handle, Message, e);
    }

    public static void NoCallbackRegistered(this ILogger logger, TrackingId Handle, string Message, Exception? e = null)
    {
        noCallbackRegistered(logger, Handle, Message, e);
    }

    public static void DbContextException(this ILogger logger, TrackingId Handle, string TypeName, string Message, Exception? e = null)
    {
        dbContextException(logger, Handle, TypeName, Message, e);
    }

    public static void CallbackInvokeFailed(this ILogger logger, TrackingId Handle, string TypeName, string Message, Exception? e = null)
    {
        callbackInvokeFailed(logger, Handle, TypeName, Message, e);
    }

    public static void InvokeCommandFailed(this ILogger logger, TrackingId Handle, string TypeName, string Message, Exception? e = null)
    {
        invokeCommandFailed(logger, Handle, TypeName, Message, e);
    }

    public static void CommandHandlerNotFound(this ILogger logger, TrackingId Handle, string TypeName, string Message, Exception? e = null)
    {
        commandHandlerNotFound(logger, Handle, TypeName, Message, e);
    }

    public static void EntityAlreadyExists(this ILogger logger, TrackingId Handle, string TypeName, string Identifier, Exception? e = null)
    {
        entityAlreadyExists(logger, Handle, TypeName, Identifier, e);
    }

    public static void EntityNotFound(this ILogger logger, TrackingId Handle, string TypeName, string Identifier, Exception? e = null)
    {
        entityNotFound(logger, Handle, TypeName, Identifier, e);
    }

    public const string EntityExistsFormat = "Track {Handle} : There already is an entity in {TypeName} with this reference: {Identifier}";
    public const string EntityNotFoundFormat = "Track {Handle} : There is no entity in {TypeName} with this reference: {Identifier}";
    private const string CommandHandlerNotFoundFormat = "Track {Handle} : No handler for {TypeName}, message is not handled:\n {Message}";
    private const string DefaultMessageFormat = "Track {Handle} : {Message}";
    private const string InvokeFailedFormat = "Track {Handle} : {TypeName} invoke or callback failed for message:\n {Message}";

    private static readonly Action<ILogger, Guid, string, Exception?> trackingInfo =
    LoggerMessage.Define<Guid, string>(
        LogLevel.Information,
        new EventId(id: 10023, name: nameof(TrackingInfo)),
        formatString: DefaultMessageFormat);

    private static readonly Action<ILogger, Guid, string, Exception?> threadTimeout =
        LoggerMessage.Define<Guid, string>(
            LogLevel.Critical,
            new EventId(id: 10024, name: nameof(ThreadTimeout)),
            formatString: DefaultMessageFormat);

    private static readonly Action<ILogger, Guid, string, Exception?> noCallbackRegistered =
        LoggerMessage.Define<Guid, string>(
            LogLevel.Information,
            new EventId(id: 10025, name: nameof(NoCallbackRegistered)),
            formatString: DefaultMessageFormat);

    private static readonly Action<ILogger, Guid, string, string, Exception?> callbackInvokeFailed =
        LoggerMessage.Define<Guid, string, string>(
            LogLevel.Warning,
            new EventId(id: 10026, name: nameof(CallbackInvokeFailed)),
            formatString: InvokeFailedFormat);

    private static readonly Action<ILogger, Guid, string, string, Exception?> invokeCommandFailed =
        LoggerMessage.Define<Guid, string, string>(
            LogLevel.Error,
            new EventId(id: 10027, name: nameof(InvokeCommandFailed)),
            formatString: InvokeFailedFormat);

    private static readonly Action<ILogger, Guid, string, string, Exception?> commandHandlerNotFound =
        LoggerMessage.Define<Guid, string, string>(
            LogLevel.Information,
            new EventId(id: 10028, name: nameof(CommandHandlerNotFound)),
            formatString: CommandHandlerNotFoundFormat);

    private static readonly Action<ILogger, Guid, string, string, Exception?> entityAlreadyExists =
        LoggerMessage.Define<Guid, string, string>(
            LogLevel.Error,
            new EventId(id: 10029, name: nameof(EntityAlreadyExists)),
            formatString: EntityExistsFormat);

    private static readonly Action<ILogger, Guid, string, string, Exception?> entityNotFound =
        LoggerMessage.Define<Guid, string, string>(
            LogLevel.Error,
            new EventId(id: 10030, name: nameof(EntityNotFound)),
            formatString: EntityNotFoundFormat);

    private static readonly Action<ILogger, Guid, string, string, Exception?> dbContextException =
        LoggerMessage.Define<Guid, string, string>(
            LogLevel.Warning,
            new EventId(id: 10031, name: nameof(DbContextException)),
            formatString: InvokeFailedFormat);
}