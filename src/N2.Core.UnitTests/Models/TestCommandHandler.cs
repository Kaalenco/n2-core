using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using N2.Core.Commands;

namespace N2.Core.UnitTests.Models;

public sealed class TestCommandHandler :
    ICommandHandler,
    ICommandHandler<TestCommand>,
    ICommandHandler<TestCommandToo>
{
    public bool IsActive => true;
    public bool IsEnabled => true;

    public bool ProcessingError => false;
    public int TimeoutInMilliSeconds { get; set; }

    private readonly Stopwatch _timer = new();
    private readonly IConductor _conductor;

    public TestCommandHandler(IConductor conductor)
    {
        _conductor = conductor;
    }

    public bool CanHandle(ICommandRequest command)
    {
        return command is TestCommand
            || command is TestCommandToo;
    }

    public ResponseStatus Invoke(ICommandRequest command)
    {
        if (command == null)
        {
            return ResponseStatus.BadRequest;
        }

        Func<ICommandRequest, ResponseStatus>? action = ToMethod(command.GetType().Name);
        if (action == null)
        {
            return ResponseStatus.NotAcceptable;
        }

        action.Invoke(command);
        return ResponseStatus.Accepted;
    }

    private Func<ICommandRequest, ResponseStatus>? ToMethod([NotNull] string type) => type switch
    {
        nameof(TestCommand) => InvokeBase,
        nameof(TestCommandToo) => InvokeToo,
        _ => null
    };

    public ResponseStatus InvokeToo([NotNull] ICommandRequest command)
    {
        TestCommandToo action = (TestCommandToo)command;
        _ = Guid.TryParse(action.Handle, out Guid handle);
        _timer.Restart();
        while (_timer.ElapsedMilliseconds < action.WaitTime)
        {
            Thread.Sleep(10);
        }
        TestResponse result = new(handle, 200, _timer.ElapsedMilliseconds);
        _conductor.CallBack(result);
        return ResponseStatus.Success;
    }

    public ResponseStatus InvokeBase([NotNull] ICommandRequest command)
    {
        TestCommand action = (TestCommand)command;
        _ = Guid.TryParse(action.Handle, out Guid handle);
        _timer.Restart();
        while (_timer.ElapsedMilliseconds < action.WaitTime)
        {
            Thread.Sleep(10);
        }
        TestResponse result = new(handle, 200, _timer.ElapsedMilliseconds);
        _conductor.CallBack(result);
        return ResponseStatus.Success;
    }

    ResponseStatus ICommandHandler<TestCommand>.ExecuteCommand(TestCommand request) => InvokeBase(request);

    ResponseStatus ICommandHandler<TestCommandToo>.ExecuteCommand(TestCommandToo request) => InvokeToo(request);
}

public sealed class TestCommandHandlerToo :
    ICommandHandler,
    ICommandHandler<TestCommandToo>
{
    public bool IsActive => true;
    public bool IsEnabled => true;

    public bool ProcessingError => false;
    public int TimeoutInMilliSeconds { get; set; }

    private readonly IConductor _conductor;

    public TestCommandHandlerToo(IConductor conductor)
    {
        _conductor = conductor;
    }

    public bool CanHandle(ICommandRequest command)
    {
        return command is TestCommandToo;
    }

    public ResponseStatus Invoke([NotNull] ICommandRequest command)
    {
        TestCommandToo action = (TestCommandToo)command;
        _ = Guid.TryParse(action.Handle, out Guid handle);
        TestResponse result = new(handle, 200, -1);
        _conductor.CallBack(result);
        return ResponseStatus.Accepted;
    }

    ResponseStatus ICommandHandler<TestCommandToo>.ExecuteCommand(TestCommandToo request) => Invoke(request);
}