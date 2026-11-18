using Microsoft.Extensions.Logging;

using Moq;

using N2.Core.Commands;

namespace N2.Core.UnitTests;

public sealed class SimpleRequest : CommandRequest
{
    public SimpleRequest() : base(Guid.NewGuid()) { }
}

[TestClass]
public sealed class WithBaseCommandHandler
{
    private sealed class SimpleHandler : BaseCommandHandler<SimpleRequest, AcknowledgeResponse>
    {
        public SimpleHandler(ILogger logger, IRuntimeValidator<SimpleRequest> validator)
            : base(logger, validator) { }

        public override Task<AcknowledgeResponse> HandleRequest(SimpleRequest request, CancellationToken cancellationToken)
            => Task.FromResult(new AcknowledgeResponse());

        public override bool CanHandle(ICommandRequest command) => command is SimpleRequest;
    }

    private sealed class SlowHandler : BaseCommandHandler<SimpleRequest, AcknowledgeResponse>
    {
        public SlowHandler(ILogger logger, IRuntimeValidator<SimpleRequest> validator)
            : base(logger, validator) { }

        public TaskCompletionSource<bool> Cancelled { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public override async Task<AcknowledgeResponse> HandleRequest(SimpleRequest request, CancellationToken cancellationToken)
        {
            using CancellationTokenRegistration registration = cancellationToken.Register(() => Cancelled.TrySetResult(true));
            await Task.Delay(Timeout.Infinite, cancellationToken);
            return new AcknowledgeResponse();
        }

        public override bool CanHandle(ICommandRequest command) => command is SimpleRequest;
    }

    private sealed class ThrowingCallbackHandler : BaseCommandHandler<SimpleRequest, AcknowledgeResponse>
    {
        public ThrowingCallbackHandler(ILogger logger, IRuntimeValidator<SimpleRequest> validator)
            : base(logger, validator) { }

        public override async Task<AcknowledgeResponse> HandleRequest(SimpleRequest request, CancellationToken cancellationToken)
        {
            using CancellationTokenRegistration registration = cancellationToken.Register(
                () => throw new InvalidOperationException("callback failure"));
            await Task.Delay(Timeout.Infinite, cancellationToken);
            return new AcknowledgeResponse();
        }

        public override bool CanHandle(ICommandRequest command) => command is SimpleRequest;
    }

    private sealed class TokenCapturingHandler : BaseCommandHandler<SimpleRequest, AcknowledgeResponse>
    {
        public TokenCapturingHandler(ILogger logger, IRuntimeValidator<SimpleRequest> validator)
            : base(logger, validator) { }

        public CancellationToken ReceivedToken { get; private set; }

        public override Task<AcknowledgeResponse> HandleRequest(SimpleRequest request, CancellationToken cancellationToken)
        {
            ReceivedToken = cancellationToken;
            return Task.FromResult(new AcknowledgeResponse());
        }

        public override bool CanHandle(ICommandRequest command) => command is SimpleRequest;
    }

    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    [TestMethod]
    public void DefaultTimeoutInMillisecondsIs200()
    {
        Assert.AreEqual(200, SimpleHandler.DefaultTimeoutInMilliseconds);
    }

    [TestMethod]
    public void LongTimeoutIs600000()
    {
        Assert.AreEqual(600_000, SimpleHandler.LongTimeout);
    }

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    [TestMethod]
    public void ConstructorNullLoggerThrows()
    {
        Mock<IRuntimeValidator<SimpleRequest>> validator = new();
        Assert.Throws<ArgumentException>(() => new SimpleHandler(null!, validator.Object));
    }

    [TestMethod]
    public void ConstructorValidArgumentsSucceeds()
    {
        Mock<ILogger> logger = new();
        Mock<IRuntimeValidator<SimpleRequest>> validator = new();
        SimpleHandler sut = new(logger.Object, validator.Object);
        Assert.IsNotNull(sut);
    }

    // -------------------------------------------------------------------------
    // Default virtual property values
    // -------------------------------------------------------------------------

    [TestMethod]
    public void IsActiveReturnsFalse()
    {
        Mock<ILogger> logger = new();
        Mock<IRuntimeValidator<SimpleRequest>> validator = new();
        SimpleHandler sut = new(logger.Object, validator.Object);
        Assert.IsFalse(sut.IsActive);
    }

    [TestMethod]
    public void IsEnabledReturnsTrue()
    {
        Mock<ILogger> logger = new();
        Mock<IRuntimeValidator<SimpleRequest>> validator = new();
        SimpleHandler sut = new(logger.Object, validator.Object);
        Assert.IsTrue(sut.IsEnabled);
    }

    // -------------------------------------------------------------------------
    // ExecuteCommand
    // -------------------------------------------------------------------------

    [TestMethod]
    public void ExecuteCommandValidRequestReturnsAccepted()
    {
        Mock<ILogger> logger = new();
        Mock<IRuntimeValidator<SimpleRequest>> validator = new();
        SimpleHandler sut = new(logger.Object, validator.Object);
        ResponseStatus result = sut.ExecuteCommand(new SimpleRequest());
        Assert.AreEqual(ResponseStatus.Success, result);
    }

    [TestMethod]
    public async Task ExecuteCommandTimeoutCancelsHandlerToken()
    {
        Mock<ILogger> logger = new();
        Mock<IRuntimeValidator<SimpleRequest>> validator = new();
        SlowHandler sut = new(logger.Object, validator.Object) { TimeoutInMilliSeconds = 50 };

        ResponseStatus result = sut.ExecuteCommand(new SimpleRequest());

        Assert.AreEqual(ResponseStatus.TimeOut, result);
        Task completed = await Task.WhenAny(sut.Cancelled.Task, Task.Delay(5000, TestContext.CancellationToken));
        Assert.AreSame(sut.Cancelled.Task, completed, "The handler token was not cancelled on timeout.");
    }

    [TestMethod]
    public void ExecuteCommandTimeoutWithThrowingCancelCallbackReturnsTimeOut()
    {
        Mock<ILogger> logger = new();
        Mock<IRuntimeValidator<SimpleRequest>> validator = new();
        ThrowingCallbackHandler sut = new(logger.Object, validator.Object) { TimeoutInMilliSeconds = 50 };

        ResponseStatus result = sut.ExecuteCommand(new SimpleRequest());

        Assert.AreEqual(ResponseStatus.TimeOut, result);
    }

    // -------------------------------------------------------------------------
    // WaitFor
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task WaitForPassesCancellationTokenToHandler()
    {
        Mock<ILogger> logger = new();
        Mock<IRuntimeValidator<SimpleRequest>> validator = new();
        TokenCapturingHandler sut = new(logger.Object, validator.Object);
        using CancellationTokenSource cts = new();

        AcknowledgeResponse result = await sut.WaitFor(new SimpleRequest(), cts.Token);

        Assert.IsNotNull(result);
        Assert.AreEqual(cts.Token, sut.ReceivedToken);
    }

    public TestContext TestContext { get; set; } = null!;
}
