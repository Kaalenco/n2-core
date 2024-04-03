using N2.Core.Extensions;

namespace N2.Core.UnitTests.Extensions;

[TestClass]
public class StringExtensionTests
{
    [DataTestMethod]
    [DataRow("00000000-0000-0000-0000-000000000000")]
    [DataRow("")]
    [DataRow(null)]
    public void ConvertToGuid_EmptyString_ReturnsEmptyGuid(string value)
    {
        var result = value.ConvertToGuid();
        Assert.AreEqual(Guid.Empty, result);
    }

    [DataTestMethod]
    [DataRow("00000000-0000-0000-0000-000000000001", "00000000-0000-0000-0000-000000000001")]
    [DataRow("Test", "54534554-0000-0000-0000-0004dada00ff")]
    public void ConvertToGuid_ValidString_ReturnsGuid(string value, string expected)
    {
        var result = value.ConvertToGuid();
        Assert.AreEqual(Guid.Parse(expected), result);
    }

    [TestMethod]
    public void ConvertToGuid_ReturnsSameGuidForUpperAndLowercase()
    {
        var result = "Test".ConvertToGuid();
        var expected = "TEST".ConvertToGuid();
        Assert.AreEqual(expected, result);
    }

    [DataTestMethod]
    [DataRow("00000000-0000-0000-0000-000000000000", "")]
    [DataRow("54534554-0000-0000-0000-0004dada00ff", "TEST")]
    [DataRow("5AA3BE99-32B9-4793-B1CF-AAA5F323BAB6", "5aa3be99-32b9-4793-b1cf-aaa5f323bab6")]
    public void ConvertToString_Should_ReturnStringValue(string value, string expected)
    {
        var guidValue = Guid.Parse(value);
        var result = guidValue.ConvertToString();
        Assert.AreEqual(expected, result);
    }

    [DataTestMethod]
    [DataRow("(3x5)+(3*2)", '(', ')', 0)]
    [DataRow("(3x5)+(3*2", '(', ')', 1)]
    [DataRow("(3x5)+ 3*2)", '(', ')', -1)]
    [DataRow("3x5)+(3*2", '(', ')', 0)]
    public void CheckBalanceTest(string value, char start, char end, int expected)
    {
        var result = value.CheckBalance(start, end);
        Assert.AreEqual(expected, result);
    }
}