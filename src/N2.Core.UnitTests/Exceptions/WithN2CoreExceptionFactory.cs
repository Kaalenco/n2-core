using N2.Core.Exceptions;

namespace N2.Core.UnitTests.Exceptions;

[TestClass]
public sealed class WithN2CoreExceptionFactory
{
    private readonly N2CoreExceptionFactory sut = new();

    // -------------------------------------------------------------------------
    // ThrowCoreException
    // -------------------------------------------------------------------------

    [TestMethod]
    public void ThrowCoreExceptionThrowsN2CoreException()
    {
        Assert.Throws<N2CoreException>(() => sut.ThrowCoreException());
    }

    // -------------------------------------------------------------------------
    // ThrowUnauthorizedException
    // -------------------------------------------------------------------------

    [TestMethod]
    public void ThrowUnauthorizedExceptionThrowsWithMessage()
    {
        UnauthorizedException ex = Assert.Throws<UnauthorizedException>(
            () => sut.ThrowUnauthorizedException("access denied"));
        Assert.AreEqual("access denied", ex.Message);
    }

    // -------------------------------------------------------------------------
    // ThrowConnectionStringNotFound
    // -------------------------------------------------------------------------

    [TestMethod]
    public void ThrowConnectionStringNotFoundThrowsConfigurationException()
    {
        ConfigurationException ex = Assert.Throws<ConfigurationException>(
            () => sut.ThrowConnectionStringNotFound("db-main"));
        Assert.IsTrue(ex.Message.Contains("db-main", StringComparison.Ordinal));
    }

    // -------------------------------------------------------------------------
    // ThrowDirectoryNotFoundException
    // -------------------------------------------------------------------------

    [TestMethod]
    public void ThrowDirectoryNotFoundExceptionThrowsConfigurationException()
    {
        ConfigurationException ex = Assert.Throws<ConfigurationException>(
            () => sut.ThrowDirectoryNotFoundException("/var/data"));
        Assert.IsTrue(ex.Message.Contains("/var/data", StringComparison.Ordinal));
    }

    // -------------------------------------------------------------------------
    // ThrowElementNotFoundException
    // -------------------------------------------------------------------------

    [TestMethod]
    public void ThrowElementNotFoundExceptionThrowsElementNotFoundException()
    {
        Assert.Throws<ElementNotFoundException>(
            () => sut.ThrowElementNotFoundException("user-42"));
    }

    // -------------------------------------------------------------------------
    // ThrowInvalidOperationException
    // -------------------------------------------------------------------------

    [TestMethod]
    public void ThrowInvalidOperationExceptionThrowsWithMessage()
    {
        OperationException ex = Assert.Throws<OperationException>(
            () => sut.ThrowInvalidOperationException("operation not allowed"));
        Assert.AreEqual("operation not allowed", ex.Message);
    }

    // -------------------------------------------------------------------------
    // ThrowIfNull
    // -------------------------------------------------------------------------

    [TestMethod]
    public void ThrowIfNullWithNullThrowsN2CoreException()
    {
        Assert.Throws<N2CoreException>(() => sut.ThrowIfNull(null));
    }

    [TestMethod]
    public void ThrowIfNullWithNonNullDoesNotThrow()
    {
        sut.ThrowIfNull(new object());
    }

    // -------------------------------------------------------------------------
    // Singleton properties return same instance on repeated calls
    // -------------------------------------------------------------------------

    [TestMethod]
    public void CoreExceptionPropertyReturnsSameInstance()
    {
        N2CoreException a = sut.CoreException;
        N2CoreException b = sut.CoreException;
        Assert.AreSame(a, b);
    }

    [TestMethod]
    public void ConfigurationExceptionPropertyReturnsSameInstance()
    {
        ConfigurationException a = sut.ConfigurationException;
        ConfigurationException b = sut.ConfigurationException;
        Assert.AreSame(a, b);
    }

    [TestMethod]
    public void UnauthorizedExceptionPropertyReturnsSameInstance()
    {
        UnauthorizedException a = sut.UnauthorizedException;
        UnauthorizedException b = sut.UnauthorizedException;
        Assert.AreSame(a, b);
    }
}
