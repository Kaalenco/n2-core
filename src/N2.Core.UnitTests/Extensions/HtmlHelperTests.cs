using N2.Core.Extensions;

namespace N2.Core.UnitTests.Extensions;

[TestClass]
public class HtmlHelperTests
{
    [DataTestMethod]
    [DataRow("h1", "Hello", "")]
    [DataRow("h1", "<h1>Hello</h1>", "Hello")]
    [DataRow("h1", "<h1 class=\"H1\">Hello</h1>", "Hello")]
    [DataRow("h1", "<html><head></head><body><h1>Hello</h1></body></html>", "Hello")]
    [DataRow("H1", "<html><head></head><body><h1>Hello</h1></body></html>", "Hello")]
    [DataRow("H1", "<html>\n\r<head>\n\r</head>\n\r<body>\n\r<h1>Hello</h1>\n\r</body>\n\r</html>\n\r", "Hello")]
    public void TestFindHtmlPart(string tagName, string source, string expected) => Assert.AreEqual(expected, StringExtensions.FindHtmlPart(tagName, source));
}