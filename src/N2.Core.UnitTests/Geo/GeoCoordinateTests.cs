using N2.Core.Geo;

namespace N2.Core.UnitTests.Geo;
[TestClass]
public class GeoCoordinateTests
{
    private GeoCoordinate UnitUnderTest = new();

    [TestMethod]
    public void GeoCoordinate_ConstructorWithDefaultValues_DoesNotThrow()
    {
        UnitUnderTest = new GeoCoordinate(Double.NaN, Double.NaN, Double.NaN, Double.NaN, Double.NaN, Double.NaN, Double.NaN);
        Assert.IsNotNull(UnitUnderTest);
    }

    [TestMethod]
    public void GeoCoordinate_ConstructorWithParameters_ReturnsInstanceWithExpectedValues()
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
    public void GeoCoordinate_DefaultConstructor_ReturnsInstanceWithDefaultValues()
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
    public void GeoCoordinate_EqualsOperatorWithNullParameters_DoesNotThrow()
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
    public void GeoCoordinate_EqualsTwoInstancesWithDifferentValuesExceptLongitudeAndLatitude_ReturnsTrue()
    {
        var first = new GeoCoordinate(11, 12, 13, 14, 15, 16, 17);
        var second = new GeoCoordinate(11, 12, 14, 15, 16, 17, 18);

        Assert.IsTrue(first.Equals(second));
    }

    [TestMethod]
    public void GeoCoordinate_EqualsTwoInstancesWithSameValues_ReturnsTrue()
    {
        var first = new GeoCoordinate(11, 12, 13, 14, 15, 16, 17);
        var second = new GeoCoordinate(11, 12, 13, 14, 15, 16, 17);

        Assert.IsTrue(first.Equals(second));
    }

    [TestMethod]
    public void GeoCoordinate_EqualsWithOtherTypes_ReturnsFalse()
    {
        var something = new int?(42);
        Assert.IsFalse(UnitUnderTest.Equals(something));
    }

    [TestMethod]
    public void GeoCoordinate_GetDistanceTo_ReturnsExpectedDistance()
    {
        var start = new GeoCoordinate(1, 1);
        var end = new GeoCoordinate(5, 5);
        var distance = start.GetDistanceTo(end);
        const double expected = 629060.759879635;
        var delta = distance - expected;

        Assert.IsTrue(delta < 1e-8);
    }

    [TestMethod]
    public void GeoCoordinate_GetDistanceToWithNaNCoordinates_ThrowsArgumentException()
    {
        Assert.ThrowsException<ArgumentException>(() => new GeoCoordinate(Double.NaN, 1).GetDistanceTo(new GeoCoordinate(5, 5)));
        Assert.ThrowsException<ArgumentException>(() => new GeoCoordinate(1, Double.NaN).GetDistanceTo(new GeoCoordinate(5, 5)));
        Assert.ThrowsException<ArgumentException>(() => new GeoCoordinate(1, 1).GetDistanceTo(new GeoCoordinate(Double.NaN, 5)));
        Assert.ThrowsException<ArgumentException>(() => new GeoCoordinate(1, 1).GetDistanceTo(new GeoCoordinate(5, Double.NaN)));
    }

    [TestMethod]
    public void GeoCoordinate_GetHashCode_OnlyReactsOnLongitudeAndLatitude()
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
    public void GeoCoordinate_GetHashCode_SwitchingLongitudeAndLatitudeReturnsSameHashCodes()
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
    public void GeoCoordinate_IsUnknownIfLongitudeAndLatitudeIsNaN_ReturnsTrue()
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
    public void GeoCoordinate_NotEqualsOperatorWithNullParameters_DoesNotThrow()
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
    public void GeoCoordinate_SetAltitude_ReturnsCorrectValue()
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
    public void GeoCoordinate_SetCourse_ThrowsOnInvalidValues()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => UnitUnderTest.Course = -0.1);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => UnitUnderTest.Course = 360.1);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new GeoCoordinate(Double.NaN, Double.NaN, Double.NaN, Double.NaN, Double.NaN, Double.NaN, -0.1));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new GeoCoordinate(Double.NaN, Double.NaN, Double.NaN, Double.NaN, Double.NaN, Double.NaN, 360.1));
    }

    [TestMethod]
    public void GeoCoordinate_SetHorizontalAccuracy_ThrowsOnInvalidValues()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => UnitUnderTest.HorizontalAccuracy = -0.1);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new GeoCoordinate(Double.NaN, Double.NaN, Double.NaN, -0.1, Double.NaN, Double.NaN, Double.NaN));
    }

    [TestMethod]
    public void GeoCoordinate_SetHorizontalAccuracyToZero_ReturnsNaNInProperty()
    {
        UnitUnderTest = new GeoCoordinate(Double.NaN, Double.NaN, Double.NaN, 0, Double.NaN, Double.NaN, Double.NaN);
        Assert.AreEqual(Double.NaN, UnitUnderTest.HorizontalAccuracy);
    }

    [TestMethod]
    public void GeoCoordinate_SetLatitude_ThrowsOnInvalidValues()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => UnitUnderTest.Latitude = 90.1);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => UnitUnderTest.Latitude = -90.1);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new GeoCoordinate(90.1, Double.NaN));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new GeoCoordinate(-90.1, Double.NaN));
    }

    [TestMethod]
    public void GeoCoordinate_SetLongitude_ThrowsOnInvalidValues()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => UnitUnderTest.Longitude = 180.1);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => UnitUnderTest.Longitude = -180.1);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new GeoCoordinate(Double.NaN, 180.1));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new GeoCoordinate(Double.NaN, -180.1));
    }

    [TestMethod]
    public void GeoCoordinate_SetSpeed_ThrowsOnInvalidValues()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => UnitUnderTest.Speed = -0.1);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new GeoCoordinate(Double.NaN, Double.NaN, Double.NaN, Double.NaN, Double.NaN, -1, Double.NaN));
    }

    [TestMethod]
    public void GeoCoordinate_SetVerticalAccuracy_ThrowsOnInvalidValues()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => UnitUnderTest.VerticalAccuracy = -0.1);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new GeoCoordinate(Double.NaN, Double.NaN, Double.NaN, Double.NaN, -0.1, Double.NaN, Double.NaN));
    }

    [TestMethod]
    public void GeoCoordinate_SetVerticalAccuracyToZero_ReturnsNaNInProperty()
    {
        UnitUnderTest = new GeoCoordinate(Double.NaN, Double.NaN, Double.NaN, Double.NaN, 0, Double.NaN, Double.NaN);
        Assert.AreEqual(Double.NaN, UnitUnderTest.VerticalAccuracy);
    }

    [TestMethod]
    public void GeoCoordinate_ToString_ReturnsLongitudeAndLatitude()
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