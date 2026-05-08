using N2.Core.Commands;
using N2.Core.Identity;

namespace N2.Core.UnitTests.Identity;

[TestClass]
public sealed class ForWebTokenExceptionUnitTests
{
    // -------------------------------------------------------------------------
    // Default constructor
    // -------------------------------------------------------------------------

    [TestMethod]
    public void DefaultCtorSetsUnauthorized()
    {
        WebTokenException ex = new();
        Assert.AreEqual(ResponseStatus.Unauthorized, ex.ResponseStatus);
        Assert.IsFalse(string.IsNullOrEmpty(ex.Message));
    }

    // -------------------------------------------------------------------------
    // String constructor
    // -------------------------------------------------------------------------

    [TestMethod]
    public void StringCtorPreservesMessage()
    {
        WebTokenException ex = new("token expired");
        Assert.AreEqual("token expired", ex.Message);
    }

    // -------------------------------------------------------------------------
    // String + inner exception constructor
    // -------------------------------------------------------------------------

    [TestMethod]
    public void StringInnerCtorChainsCorrectly()
    {
        InvalidOperationException inner = new("inner");
        WebTokenException ex = new("outer", inner);
        Assert.AreEqual("outer", ex.Message);
        Assert.AreEqual(inner, ex.InnerException);
    }

    // -------------------------------------------------------------------------
    // Type hierarchy
    // -------------------------------------------------------------------------

    [TestMethod]
    public void IsAssignableToException()
    {
        WebTokenException ex = new();
        Assert.IsInstanceOfType<Exception>(ex);
    }
}
