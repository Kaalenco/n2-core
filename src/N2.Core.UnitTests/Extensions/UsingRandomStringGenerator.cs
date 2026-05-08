using N2.Core.Extensions;

namespace N2.Core.UnitTests.Extensions;

[TestClass]
public sealed class UsingRandomStringGenerator
{
    private const string AllowedChars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";

    // -------------------------------------------------------------------------
    // Generate
    // -------------------------------------------------------------------------

    [TestMethod]
    public void GenerateReturnsStringOfRequestedLength()
    {
        string result = RandomStringGenerator.Generate(12);
        Assert.AreEqual(12, result.Length);
    }

    [TestMethod]
    public void GenerateWithZeroLengthReturnsEmpty()
    {
        string result = RandomStringGenerator.Generate(0);
        Assert.AreEqual(0, result.Length);
    }

    [TestMethod]
    public void GenerateReturnsOnlyAllowedCharacters()
    {
        string result = RandomStringGenerator.Generate(50);
        foreach (char c in result)
        {
            Assert.IsTrue(AllowedChars.Contains(c), $"Unexpected character: '{c}'");
        }
    }

    [TestMethod]
    public void GenerateTwoCallsSameLengthReturnDifferentStrings()
    {
        string first = RandomStringGenerator.Generate(16);
        string second = RandomStringGenerator.Generate(16);
        Assert.AreNotEqual(first, second);
    }

    [TestMethod]
    public void GenerateExcludesEasilyConfusedCharacters()
    {
        string result = RandomStringGenerator.Generate(200);
        Assert.IsFalse(result.Contains('0'), "Should not contain '0'");
        Assert.IsFalse(result.Contains('1'), "Should not contain '1'");
        Assert.IsFalse(result.Contains('I'), "Should not contain 'I'");
        Assert.IsFalse(result.Contains('O'), "Should not contain 'O'");
        Assert.IsFalse(result.Contains('l'), "Should not contain 'l'");
    }
}
