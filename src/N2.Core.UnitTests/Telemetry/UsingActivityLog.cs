using System.Diagnostics;

using N2.Core.Telemetry;

namespace N2.Core.UnitTests.Telemetry;

[TestClass]
public sealed class UsingActivityLog
{
    // -------------------------------------------------------------------------
    // Constructors
    // -------------------------------------------------------------------------

    [TestMethod]
    public void DefaultCtorCreatesInstance()
    {
        using ActivityLog sut = new();
        Assert.IsNotNull(sut);
    }

    [TestMethod]
    public void ActivityCtorAcceptsNullActivity()
    {
        using ActivityLog sut = new(null);
        Assert.IsNotNull(sut);
    }

    // -------------------------------------------------------------------------
    // Tag
    // -------------------------------------------------------------------------

    [TestMethod]
    public void TagWithNoActivityDoesNotThrow()
    {
        using ActivityLog sut = new();
        sut.Tag("key", "value");
    }

    [TestMethod]
    public void TagWithActivitySetsTagOnActivity()
    {
        using Activity activity = new("test");
        activity.Start();
        using ActivityLog sut = new(activity);
        sut.Tag("env", "prod");
        Assert.AreEqual("prod", activity.GetTagItem("env"));
    }

    [TestMethod]
    public void TagNullValueStoresNullStringOnActivity()
    {
        using Activity activity = new("test");
        activity.Start();
        using ActivityLog sut = new(activity);
        sut.Tag("env", null);
        Assert.AreEqual("NULL", activity.GetTagItem("env"));
    }

    // -------------------------------------------------------------------------
    // SetActivity
    // -------------------------------------------------------------------------

    [TestMethod]
    public void SetActivityReplacesActivityForSubsequentTags()
    {
        using Activity activity = new("test");
        activity.Start();
        using ActivityLog sut = new();
        sut.SetActivity(activity);
        sut.Tag("region", "eu-west");
        Assert.AreEqual("eu-west", activity.GetTagItem("region"));
    }

    // -------------------------------------------------------------------------
    // TagError
    // -------------------------------------------------------------------------

    [TestMethod]
    public void TagErrorWithNullDoesNotThrow()
    {
        using ActivityLog sut = new();
        sut.TagError(null!);
    }

    [TestMethod]
    public void TagErrorWithExceptionAddsEventToActivity()
    {
        using Activity activity = new("test");
        activity.Start();
        using ActivityLog sut = new(activity);
        sut.TagError(new InvalidOperationException("boom"));
        Assert.AreEqual(1, activity.Events.Count());
    }

    [TestMethod]
    public void TagErrorWithNoActivityDoesNotThrow()
    {
        using ActivityLog sut = new();
        sut.TagError(new InvalidOperationException("boom"));
    }

    // -------------------------------------------------------------------------
    // Dispose
    // -------------------------------------------------------------------------

    [TestMethod]
    public void DisposeCanBeCalledTwice()
    {
        using ActivityLog sut = new();
        sut.Dispose();
        sut.Dispose();
    }

    [TestMethod]
    public void DisposeWithActivityDisposesActivity()
    {
        using Activity activity = new("test");
        activity.Start();
        ActivityLog sut = new(activity);
        sut.Dispose();
        Assert.AreEqual(ActivityStatusCode.Unset, activity.Status);
    }
}
