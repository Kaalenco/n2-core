using N2.Core.Geo;

namespace N2.Core.UnitTests.Geo;

[TestClass]
public sealed class WithGeoPosition
{
    // -------------------------------------------------------------------------
    // Default constructor
    // -------------------------------------------------------------------------

    [TestMethod]
    public void DefaultCtorLeavesPropertiesAtDefaults()
    {
        GeoPosition<string> sut = new();
        Assert.IsNull(sut.Location);
        Assert.AreEqual(default(DateTimeOffset), sut.Timestamp);
    }

    // -------------------------------------------------------------------------
    // Parameterised constructor
    // -------------------------------------------------------------------------

    [TestMethod]
    public void ParameterisedCtorSetsTimestampAndLocation()
    {
        DateTimeOffset ts = new(2024, 6, 1, 12, 0, 0, TimeSpan.Zero);
        GeoPosition<string> sut = new(ts, "Oslo");
        Assert.AreEqual(ts, sut.Timestamp);
        Assert.AreEqual("Oslo", sut.Location);
    }

    [TestMethod]
    public void ParameterisedCtorAcceptsNullLocation()
    {
        DateTimeOffset ts = DateTimeOffset.UtcNow;
        GeoPosition<string> sut = new(ts, null!);
        Assert.AreEqual(ts, sut.Timestamp);
        Assert.IsNull(sut.Location);
    }

    // -------------------------------------------------------------------------
    // Properties are mutable after construction
    // -------------------------------------------------------------------------

    [TestMethod]
    public void LocationCanBeSetAfterConstruction()
    {
        GeoPosition<int> sut = new();
        sut.Location = 42;
        Assert.AreEqual(42, sut.Location);
    }

    [TestMethod]
    public void TimestampCanBeSetAfterConstruction()
    {
        DateTimeOffset ts = DateTimeOffset.UtcNow;
        GeoPosition<int> sut = new();
        sut.Timestamp = ts;
        Assert.AreEqual(ts, sut.Timestamp);
    }
}
