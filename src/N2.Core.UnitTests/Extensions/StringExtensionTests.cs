using System.Globalization;

using N2.Core.Extensions;

namespace N2.Core.UnitTests.Extensions;

[TestClass]
public class StringExtensionTests
{
    // -------------------------------------------------------------------------
    // ConvertToGuid
    // -------------------------------------------------------------------------

    [TestMethod]
    [DataRow("00000000-0000-0000-0000-000000000000")]
    [DataRow("")]
    [DataRow(null)]
    public void ConvertToGuidEmptyStringReturnsEmptyGuid(string value)
    {
        Guid result = value.ConvertToGuid();
        Assert.AreEqual(Guid.Empty, result);
    }

    [TestMethod]
    [DataRow("00000000-0000-0000-0000-000000000001", "00000000-0000-0000-0000-000000000001")]
    [DataRow("Test", "54534554-0000-0000-0000-0004dada00ff")]
    public void ConvertToGuidValidStringReturnsGuid(string value, string expected)
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

    // -------------------------------------------------------------------------
    // ConvertToString
    // -------------------------------------------------------------------------

    [TestMethod]
    [DataRow("00000000-0000-0000-0000-000000000000", "")]
    [DataRow("54534554-0000-0000-0000-0004dada00ff", "TEST")]
    [DataRow("5AA3BE99-32B9-4793-B1CF-AAA5F323BAB6", "5aa3be99-32b9-4793-b1cf-aaa5f323bab6")]
    public void ConvertToStringShouldReturnStringValue(string value, string expected)
    {
        Guid guidValue = Guid.Parse(value);
        string result = guidValue.ConvertToString();
        Assert.AreEqual(expected, result);
    }

    // -------------------------------------------------------------------------
    // CheckBalance
    // -------------------------------------------------------------------------

    [TestMethod]
    [DataRow("(3x5)+(3*2)", '(', ')', 0)]
    [DataRow("(3x5)+(3*2", '(', ')', 1)]
    [DataRow("(3x5)+ 3*2)", '(', ')', -1)]
    [DataRow("3x5)+(3*2", '(', ')', 0)]
    public void CheckBalanceReturnsExpectedCount(string value, char start, char end, int expected)
    {
        int result = value.CheckBalance(start, end);
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    public void CheckBalanceWithNullOrEmptyReturnsZero(string? value)
    {
        int result = value!.CheckBalance('(', ')');
        Assert.AreEqual(0, result);
    }

    // -------------------------------------------------------------------------
    // ParseStringToLocalTime
    // -------------------------------------------------------------------------

    [TestMethod]
    [DataRow("2024-01-01", "2024-01-01T01:00:00")]
    [DataRow("2024-05-01", "2024-05-01T02:00:00")]
    public void ConvertToDateTimeShouldReturnDateTime(string value, string expected)
    {
        DateTimeOffset parsedOffset = value.ParseStringToLocalTime(CultureInfo.InvariantCulture);
        DateTime result = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(parsedOffset.DateTime, "W. Europe Standard Time");
        Assert.AreEqual(expected, result.ToString("s"));
    }

    // -------------------------------------------------------------------------
    // Soundex
    // -------------------------------------------------------------------------

    [TestMethod]
    [DataRow("Abacadabra", "A123")]
    [DataRow("ABACADABRA", "A123")]
    [DataRow("Kaal", "K400")]
    [DataRow("Balt", "B430")]
    public void SoundexReturnsExpectedCode(string value, string expected)
    {
        string soundex = value.Soundex();
        Assert.AreEqual(expected, soundex);
    }

    // -------------------------------------------------------------------------
    // IsEnum
    // -------------------------------------------------------------------------

    [TestMethod]
    [DataRow("1")]
    [DataRow("first")]
    [DataRow("First")]
    [DataRow("FIRST")]
    public void IsEnumReturnsTrueWhenValueMatchesEnum(string value)
    {
        bool result = value.IsEnum(StringExtensionTestsControlSet.First);
        Assert.IsTrue(result);
    }

    [TestMethod]
    [DataRow("0")]
    [DataRow("Second")]
    [DataRow("Third")]
    [DataRow("")]
    [DataRow(null)]
    public void IsEnumReturnsFalseWhenValueDoesNotMatchEnum(string? value)
    {
        bool result = value!.IsEnum(StringExtensionTestsControlSet.First);
        Assert.IsFalse(result);
    }

    // -------------------------------------------------------------------------
    // UppercaseFirst
    // -------------------------------------------------------------------------

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    public void UppercaseFirstWithNullOrEmptyReturnsEmpty(string? value)
    {
        string result = value!.UppercaseFirst();
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void UppercaseFirstWithSingleCharUppercasesIt()
    {
        Assert.AreEqual("A", "a".UppercaseFirst());
    }

    [TestMethod]
    [DataRow("hello world", "Hello world")]
    [DataRow("HELLO WORLD", "Hello world")]
    [DataRow("hELLO", "Hello")]
    public void UppercaseFirstCapitalisesFirstAndLowercasesRest(string value, string expected)
    {
        Assert.AreEqual(expected, value.UppercaseFirst());
    }

    // -------------------------------------------------------------------------
    // Truncate
    // -------------------------------------------------------------------------

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    public void TruncateWithNullOrEmptyReturnsEmpty(string? value)
    {
        string result = value!.Truncate(10);
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void TruncateShorterThanLengthReturnsOriginal()
    {
        Assert.AreEqual("abc", "abc".Truncate(10));
    }

    [TestMethod]
    public void TruncateEqualToLengthReturnsOriginal()
    {
        Assert.AreEqual("abc", "abc".Truncate(3));
    }

    [TestMethod]
    public void TruncateLongerThanLengthCutsString()
    {
        Assert.AreEqual("abcde", "abcdefgh".Truncate(5));
    }

    // -------------------------------------------------------------------------
    // GetStableHashCode
    // -------------------------------------------------------------------------

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    public void GetStableHashCodeWithNullOrEmptyReturnsZero(string? value)
    {
        Assert.AreEqual(0, value!.GetStableHashCode());
    }

    [TestMethod]
    public void GetStableHashCodeReturnsSameValueForSameInput()
    {
        Assert.AreEqual("hello".GetStableHashCode(), "hello".GetStableHashCode());
    }

    [TestMethod]
    public void GetStableHashCodeReturnsDifferentValuesForDifferentInputs()
    {
        Assert.AreNotEqual("hello".GetStableHashCode(), "world".GetStableHashCode());
    }

    [TestMethod]
    public void GetStableHashCodeIsCaseSensitive()
    {
        Assert.AreNotEqual("Test".GetStableHashCode(), "test".GetStableHashCode());
    }

    // -------------------------------------------------------------------------
    // ReplaceChars
    // -------------------------------------------------------------------------

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    public void ReplaceCharsWithNullOrEmptyReturnsEmpty(string? value)
    {
        string result = value!.ReplaceChars(['a', 'b'], 'x');
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void ReplaceCharsReplacesMatchingCharacters()
    {
        Assert.AreEqual("hxllx", "hello".ReplaceChars(['e', 'o'], 'x'));
    }

    [TestMethod]
    public void ReplaceCharsLeavesNonMatchingCharactersUnchanged()
    {
        Assert.AreEqual("hello", "hello".ReplaceChars(['z'], 'x'));
    }

    [TestMethod]
    public void ReplaceCharsReplacesAllMatchingOccurrences()
    {
        Assert.AreEqual("x-x", "a-a".ReplaceChars(['a'], 'x'));
    }

    // -------------------------------------------------------------------------
    // SanitizeFileName
    // -------------------------------------------------------------------------

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    public void SanitizeFileNameWithNullOrEmptyReturnsEmpty(string? value)
    {
        Assert.AreEqual(string.Empty, value!.SanitizeFileName());
    }

    [TestMethod]
    public void SanitizeFileNameLowercasesResult()
    {
        Assert.AreEqual("filename", "FILENAME".SanitizeFileName());
    }

    [TestMethod]
    [DataRow("my file.txt", "my-file-txt")]
    [DataRow("report <2024>", "report--2024-")]
    [DataRow("path/to\\file", "path-to-file")]
    [DataRow("name:value", "name-value")]
    public void SanitizeFileNameReplacesForbiddenCharacters(string value, string expected)
    {
        Assert.AreEqual(expected, value.SanitizeFileName());
    }

    [TestMethod]
    public void SanitizeFileNamePreservesAlphanumericCharacters()
    {
        Assert.AreEqual("abc123", "abc123".SanitizeFileName());
    }

    // -------------------------------------------------------------------------
    // FindHtmlPart
    // -------------------------------------------------------------------------

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    public void FindHtmlPartWithNullOrEmptyContentReturnsEmpty(string? content)
    {
        Assert.AreEqual(string.Empty, StringExtensions.FindHtmlPart("div", content!));
    }

    [TestMethod]
    public void FindHtmlPartWithMissingTagReturnsEmpty()
    {
        Assert.AreEqual(string.Empty, StringExtensions.FindHtmlPart("div", "<span>hello</span>"));
    }

    [TestMethod]
    public void FindHtmlPartExtractsContentBetweenTags()
    {
        Assert.AreEqual("hello", StringExtensions.FindHtmlPart("div", "<div>hello</div>"));
    }

    [TestMethod]
    public void FindHtmlPartIsCaseInsensitive()
    {
        Assert.AreEqual("hello", StringExtensions.FindHtmlPart("div", "<DIV>hello</DIV>"));
    }

    [TestMethod]
    public void FindHtmlPartHandlesTagWithAttributes()
    {
        Assert.AreEqual("hello", StringExtensions.FindHtmlPart("div", "<div class=\"test\">hello</div>"));
    }

    [TestMethod]
    public void FindHtmlPartWithUnclosedTagReturnsEmpty()
    {
        Assert.AreEqual(string.Empty, StringExtensions.FindHtmlPart("div", "<div>hello"));
    }
}

public enum StringExtensionTestsControlSet
{
    None,
    First,
    Second
}
