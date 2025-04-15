using N2.Core.Geo;

namespace N2.Core.UnitTests.Geo;
[TestClass]
public class GeoCoordinateTests
{
    private GeoCoordinate UnitUnderTest = new();

    [TestMethod]
    public void GeoCoordinateConstructorWithDefaultValuesDoesNotThrow()
    {
        UnitUnderTest = new GeoCoordinate(Double.NaN, Double.NaN, Double.NaN, Double.NaN, Double.NaN, Double.NaN, Double.NaN);
        Assert.IsNotNull(UnitUnderTest);
    }

    [TestMethod]
    public void GeoCoordinateConstructorWithParametersReturnsInstanceWithExpectedValues()
    {
        const double latitude = 42D;
        const double longitude = 44D;
        const double altitude = 46D;
        const double horizontalAccuracy = 48D;
        const double verticalAccuracy = 50D;
        const double speed = 52D;
        const double course = 54D;
        const bool isUnknown = false;
        UnitUnderTest = new GeoCoordinate(latitude, longitude, altitude, horizontalAccuracy, verticalAccuracy, speed, course);

        Assert.AreEqual(latitude, UnitUnderTest.Latitude);
        Assert.AreEqual(longitude, UnitUnderTest.Longitude);
        Assert.AreEqual(altitude, UnitUnderTest.Altitude);
        Assert.AreEqual(horizontalAccuracy, UnitUnderTest.HorizontalAccuracy);
        Assert.AreEqual(verticalAccuracy, UnitUnderTest.VerticalAccuracy);
        Assert.AreEqual(speed, UnitUnderTest.Speed);
        Assert.AreEqual(course, UnitUnderTest.Course);
        Assert.AreEqual(isUnknown, UnitUnderTest.IsUnknown);
    }

    [TestMethod]
    public void GeoCoordinateDefaultConstructorReturnsInstanceWithDefaultValues()
    {
        Assert.AreEqual(Double.NaN, UnitUnderTest.Altitude);
        Assert.AreEqual(Double.NaN, UnitUnderTest.Course);
        Assert.AreEqual(Double.NaN, UnitUnderTest.HorizontalAccuracy);
        Assert.IsTrue(UnitUnderTest.IsUnknown);
        Assert.AreEqual(Double.NaN, UnitUnderTest.Latitude);
        Assert.AreEqual(Double.NaN, UnitUnderTest.Longitude);
        Assert.AreEqual(Double.NaN, UnitUnderTest.Speed);
        Assert.AreEqual(Double.NaN, UnitUnderTest.VerticalAccuracy);
    }

    [TestMethod]
    public void GeoCoordinateEqualsOperatorWithNullParametersDoesNotThrow()
    {
        GeoCoordinate? first = null;
        GeoCoordinate? second = null;
        Assert.AreEqual(first, second);

        first = new GeoCoordinate();
        Assert.AreNotEqual(first, second);

        first = null;
        second = new GeoCoordinate();
        Assert.AreNotEqual(first, second);
    }

    [TestMethod]
    public void GeoCoordinateEqualsTwoInstancesWithDifferentValuesExceptLongitudeAndLatitudeReturnsTrue()
    {
        var first = new GeoCoordinate(11, 12, 13, 14, 15, 16, 17);
        var second = new GeoCoordinate(11, 12, 14, 15, 16, 17, 18);

        Assert.IsTrue(first.Equals(second));
    }

    [TestMethod]
    public void GeoCoordinateEqualsTwoInstancesWithSameValuesReturnsTrue()
    {
        var first = new GeoCoordinate(11, 12, 13, 14, 15, 16, 17);
        var second = new GeoCoordinate(11, 12, 13, 14, 15, 16, 17);

        Assert.IsTrue(first.Equals(second));
    }

    [TestMethod]
    public void GeoCoordinateEqualsWithOtherTypesReturnsFalse()
    {
        var something = new int?(42);
        Assert.IsFalse(UnitUnderTest.Equals(something));
    }

    [TestMethod]
    public void GeoCoordinateGetDistanceToReturnsExpectedDistance()
    {
        var start = new GeoCoordinate(1, 1);
        var end = new GeoCoordinate(5, 5);
        var distance = start.GetDistanceTo(end);
        const double expected = 629060.759879635;
        var delta = distance - expected;

        Assert.IsTrue(delta < 1e-8);
    }

    [TestMethod]
    public void GeoCoordinateGetDistanceToWithNaNCoordinatesThrowsArgumentException()
    {
        Assert.ThrowsException<ArgumentException>(() => new GeoCoordinate(Double.NaN, 1).GetDistanceTo(new GeoCoordinate(5, 5)));
        Assert.ThrowsException<ArgumentException>(() => new GeoCoordinate(1, Double.NaN).GetDistanceTo(new GeoCoordinate(5, 5)));
        Assert.ThrowsException<ArgumentException>(() => new GeoCoordinate(1, 1).GetDistanceTo(new GeoCoordinate(Double.NaN, 5)));
        Assert.ThrowsException<ArgumentException>(() => new GeoCoordinate(1, 1).GetDistanceTo(new GeoCoordinate(5, Double.NaN)));
    }

    [TestMethod]
    public void GeoCoordinateGetHashCodeOnlyReactsOnLongitudeAndLatitude()
    {
        UnitUnderTest.Latitude = 2;
        UnitUnderTest.Longitude = 3;
        var firstHash = UnitUnderTest.GetHashCode();

        UnitUnderTest.Altitude = 4;
        UnitUnderTest.Course = 5;
        UnitUnderTest.HorizontalAccuracy = 6;
        UnitUnderTest.Speed = 7;
        UnitUnderTest.VerticalAccuracy = 8;
        var secondHash = UnitUnderTest.GetHashCode();

        Assert.AreEqual(firstHash, secondHash);
    }

    [TestMethod]
    public void GeoCoordinateGetHashCodeSwitchingLongitudeAndLatitudeReturnsSameHashCodes()
    {
        UnitUnderTest.Latitude = 2;
        UnitUnderTest.Longitude = 3;
        var firstHash = UnitUnderTest.GetHashCode();

        UnitUnderTest.Latitude = 3;
        UnitUnderTest.Longitude = 2;
        var secondHash = UnitUnderTest.GetHashCode();

        Assert.AreEqual(firstHash, secondHash);
    }

    [TestMethod]
    public void GeoCoordinateIsUnknownIfLongitudeAndLatitudeIsNaNReturnsTrue()
    {
        UnitUnderTest.Longitude = 1;
        UnitUnderTest.Latitude = Double.NaN;
        Assert.IsFalse(UnitUnderTest.IsUnknown);

        UnitUnderTest.Longitude = Double.NaN;
        UnitUnderTest.Latitude = 1;
        Assert.IsFalse(UnitUnderTest.IsUnknown);

        UnitUnderTest.Longitude = Double.NaN;
        UnitUnderTest.Latitude = Double.NaN;
        Assert.IsTrue(UnitUnderTest.IsUnknown);
    }

    [TestMethod]
    public void GeoCoordinateNotEqualsOperatorWithNullParametersDoesNotThrow()
    {
        GeoCoordinate? first = null;
        GeoCoordinate second = new();
        Assert.AreNotEqual(first, second);

        first = new GeoCoordinate();
        Assert.AreEqual(first, second);

        first = null;
        second = new GeoCoordinate();
        Assert.AreNotEqual(first, second);
    }

    [TestMethod]
    public void GeoCoordinateSetAltitudeReturnsCorrectValue()
    {
        Assert.AreEqual(Double.NaN, UnitUnderTest.Altitude);

        UnitUnderTest.Altitude = 0;
        Assert.AreEqual(0, UnitUnderTest.Altitude);

        UnitUnderTest.Altitude = Double.MinValue;
        Assert.AreEqual(Double.MinValue, UnitUnderTest.Altitude);

        UnitUnderTest.Altitude = Double.MaxValue;
        Assert.AreEqual(Double.MaxValue, UnitUnderTest.Altitude);
    }

    [TestMethod]
    public void GeoCoordinateSetCourseThrowsOnInvalidValues()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => UnitUnderTest.Course = -0.1);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => UnitUnderTest.Course = 360.1);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new GeoCoordinate(Double.NaN, Double.NaN, Double.NaN, Double.NaN, Double.NaN, Double.NaN, -0.1));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new GeoCoordinate(Double.NaN, Double.NaN, Double.NaN, Double.NaN, Double.NaN, Double.NaN, 360.1));
    }

    [TestMethod]
    public void GeoCoordinateSetHorizontalAccuracyThrowsOnInvalidValues()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => UnitUnderTest.HorizontalAccuracy = -0.1);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new GeoCoordinate(Double.NaN, Double.NaN, Double.NaN, -0.1, Double.NaN, Double.NaN, Double.NaN));
    }

    [TestMethod]
    public void GeoCoordinateSetHorizontalAccuracyToZeroReturnsNaNInProperty()
    {
        UnitUnderTest = new GeoCoordinate(Double.NaN, Double.NaN, Double.NaN, 0, Double.NaN, Double.NaN, Double.NaN);
        Assert.AreEqual(Double.NaN, UnitUnderTest.HorizontalAccuracy);
    }

    [TestMethod]
    public void GeoCoordinateSetLatitudeThrowsOnInvalidValues()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => UnitUnderTest.Latitude = 90.1);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => UnitUnderTest.Latitude = -90.1);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new GeoCoordinate(90.1, Double.NaN));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new GeoCoordinate(-90.1, Double.NaN));
    }

    [TestMethod]
    public void GeoCoordinateSetLongitudeThrowsOnInvalidValues()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => UnitUnderTest.Longitude = 180.1);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => UnitUnderTest.Longitude = -180.1);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new GeoCoordinate(Double.NaN, 180.1));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new GeoCoordinate(Double.NaN, -180.1));
    }

    [TestMethod]
    public void GeoCoordinateSetSpeedThrowsOnInvalidValues()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => UnitUnderTest.Speed = -0.1);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new GeoCoordinate(Double.NaN, Double.NaN, Double.NaN, Double.NaN, Double.NaN, -1, Double.NaN));
    }

    [TestMethod]
    public void GeoCoordinateSetVerticalAccuracyThrowsOnInvalidValues()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => UnitUnderTest.VerticalAccuracy = -0.1);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new GeoCoordinate(Double.NaN, Double.NaN, Double.NaN, Double.NaN, -0.1, Double.NaN, Double.NaN));
    }

    [TestMethod]
    public void GeoCoordinateSetVerticalAccuracyToZeroReturnsNaNInProperty()
    {
        UnitUnderTest = new GeoCoordinate(Double.NaN, Double.NaN, Double.NaN, Double.NaN, 0, Double.NaN, Double.NaN);
        Assert.AreEqual(Double.NaN, UnitUnderTest.VerticalAccuracy);
    }

    [TestMethod]
    public void GeoCoordinateToStringReturnsLongitudeAndLatitude()
    {
        Assert.AreEqual("Unknown", UnitUnderTest.ToString());

        UnitUnderTest.Latitude = 1;
        UnitUnderTest.Longitude = Double.NaN;
        Assert.AreEqual("1, NaN", UnitUnderTest.ToString());

        UnitUnderTest.Latitude = Double.NaN;
        UnitUnderTest.Longitude = 1;
        Assert.AreEqual("NaN, 1", UnitUnderTest.ToString());
    }

    [TestInitialize]
    public void Initialize()
    {
        UnitUnderTest = new GeoCoordinate();
    }
}