using N2.Core.Commands;
using N2.Core.Entity;

namespace N2.Core.UnitTests.Entity;

[TestClass]
public sealed class ForEntityConnectionExceptionUnitTests
{
    // -------------------------------------------------------------------------
    // Default constructor
    // -------------------------------------------------------------------------

    [TestMethod]
    public void DefaultCtorSetsServiceUnavailable()
    {
        EntityConnectionException ex = new();
        Assert.AreEqual(ResponseStatus.ServiceUnavailable, ex.ResponseStatus);
        Assert.IsFalse(string.IsNullOrEmpty(ex.Message));
    }

    [TestMethod]
    public void DefaultCtorSetsErrorCode500()
    {
        EntityConnectionException ex = new();
        Assert.AreEqual(500, ex.ErrorCode);
    }

    // -------------------------------------------------------------------------
    // String constructor
    // -------------------------------------------------------------------------

    [TestMethod]
    public void StringCtorPreservesMessage()
    {
        EntityConnectionException ex = new("connection refused");
        Assert.AreEqual("connection refused", ex.Message);
    }

    // -------------------------------------------------------------------------
    // String + inner exception constructor
    // -------------------------------------------------------------------------

    [TestMethod]
    public void StringInnerCtorChainsCorrectly()
    {
        InvalidOperationException inner = new("inner");
        EntityConnectionException ex = new("outer", inner);
        Assert.AreEqual("outer", ex.Message);
        Assert.AreEqual(inner, ex.InnerException);
    }

    // -------------------------------------------------------------------------
    // Type hierarchy
    // -------------------------------------------------------------------------

    [TestMethod]
    public void IsAssignableToException()
    {
        EntityConnectionException ex = new();
        Assert.IsInstanceOfType<Exception>(ex);
    }
}
