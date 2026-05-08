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

        public override Task<AcknowledgeResponse> HandleRequestAsync(SimpleRequest request)
            => Task.FromResult(new AcknowledgeResponse());

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
}
