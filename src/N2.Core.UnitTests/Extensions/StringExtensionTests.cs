using System.Globalization;

using N2.Core.Extensions;

namespace N2.Core.UnitTests.Extensions;

[TestClass]
public class StringExtensionTests
{
    [DataTestMethod]
    [DataRow("00000000-0000-0000-0000-000000000000")]
    [DataRow("")]
    [DataRow(null)]
    public void ConvertToGuidEmptyStringReturnsEmptyGuid(string value)
    {
        Guid result = value.ConvertToGuid();
        Assert.AreEqual(Guid.Empty, result);
    }

    [DataTestMethod]
    [DataRow("00000000-0000-0000-0000-000000000001", "00000000-0000-0000-0000-000000000001")]
    [DataRow("Test", "54534554-0000-0000-0000-0004dada00ff")]
    public void ConvertToGuidValidStringReturnsGuid(
        string value, string expected)
    {
        Guid result = value.ConvertToGuid();
        Assert.AreEqual(Guid.Parse(expected), result);
    }

    [TestMethod]
    public void ConvertToGuidReturnsSameGuidForUpperAndLowercase()
    {
        Guid result = "Test".ConvertToGuid();
        Guid expected = "TEST".ConvertToGuid();
        Assert.AreEqual(expected, result);
    }

    [DataTestMethod]
    [DataRow("00000000-0000-0000-0000-000000000000", "")]
    [DataRow("54534554-0000-0000-0000-0004dada00ff", "TEST")]
    [DataRow("5AA3BE99-32B9-4793-B1CF-AAA5F323BAB6", "5aa3be99-32b9-4793-b1cf-aaa5f323bab6")]
    public void ConvertToStringShouldReturnStringValue(
        string value, string expected)
    {
        Guid guidValue = Guid.Parse(value);
        string result = guidValue.ConvertToString();
        Assert.AreEqual(expected, result);
    }

    [DataTestMethod]
    [DataRow("(3x5)+(3*2)", '(', ')', 0)]
    [DataRow("(3x5)+(3*2", '(', ')', 1)]
    [DataRow("(3x5)+ 3*2)", '(', ')', -1)]
    [DataRow("3x5)+(3*2", '(', ')', 0)]
    public void CheckBalanceTest(
        string value, char start, char end, int expected)
    {
        int result = value.CheckBalance(start, end);
        Assert.AreEqual(expected, result);
    }

    [DataTestMethod]
    [DataRow("2024-01-01", "2024-01-01T01:00:00")]
    [DataRow("2024-05-01", "2024-05-01T02:00:00")]
    public void ConvertToDateTimeShouldReturnDateTime(
        string value, string expected)
    {
        DateTimeOffset parsedOffset = value.ParseStringToLocalTime(CultureInfo.InvariantCulture);
        DateTime result = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(parsedOffset.DateTime, "W. Europe Standard Time");

        Assert.AreEqual(expected, result.ToString("s"));
    }
}