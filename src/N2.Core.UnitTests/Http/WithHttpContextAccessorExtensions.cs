using Microsoft.Extensions.Primitives;

using Moq;

using N2.Core.Http;

namespace N2.Core.UnitTests.Http;

[TestClass]
public class WithHttpContextAccessorExtensions
{
    [TestMethod]
    public void FromQueryString_NullAccessor_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => ((IHttpContextAccessor)null!).FromQueryString<SearchQuery>());
    }

    [TestMethod]
    public void FromQueryString_NullHttpContext_ReturnsDefault()
    {
        Mock<IHttpContextAccessor> accessorMock = new();
        accessorMock.Setup(x => x.HttpContext).Returns((IHttpContext)null!);

        SearchQuery result = accessorMock.Object.FromQueryString<SearchQuery>();

        Assert.IsNull(result.Name);
        Assert.IsNull(result.Category);
    }

    [TestMethod]
    public void FromQueryString_NullQuery_ReturnsDefault()
    {
        Mock<IHttpContextAccessor> accessorMock = new();
        Mock<IHttpContext> contextMock = new();
        Mock<IHttpRequest> requestMock = new();
        accessorMock.Setup(x => x.HttpContext).Returns(contextMock.Object);
        contextMock.SetupGet(x => x.Request).Returns(requestMock.Object);
        requestMock.SetupGet(x => x.Query).Returns((IEnumerable<KeyValuePair<string, StringValues>>)null!);

        SearchQuery result = accessorMock.Object.FromQueryString<SearchQuery>();

        Assert.IsNull(result.Name);
        Assert.IsNull(result.Category);
    }

    [TestMethod]
    public void FromQueryString_MatchingProperties_MapsValues()
    {
        Mock<IHttpContextAccessor> accessorMock = BuildAccessorMock(new Dictionary<string, StringValues>
        {
            ["Name"] = "Alice",
            ["Category"] = "Books"
        });

        SearchQuery result = accessorMock.Object.FromQueryString<SearchQuery>();

        Assert.AreEqual("Alice", result.Name);
        Assert.AreEqual("Books", result.Category);
    }

    [TestMethod]
    public void FromQueryString_UnknownKey_IsIgnored()
    {
        Mock<IHttpContextAccessor> accessorMock = BuildAccessorMock(new Dictionary<string, StringValues>
        {
            ["Unknown"] = "Value"
        });

        SearchQuery result = accessorMock.Object.FromQueryString<SearchQuery>();

        Assert.IsNull(result.Name);
        Assert.IsNull(result.Category);
    }

    [TestMethod]
    public void FromQueryString_MultipleValuesForKey_IsIgnored()
    {
        Mock<IHttpContextAccessor> accessorMock = BuildAccessorMock(new Dictionary<string, StringValues>
        {
            ["Name"] = new StringValues(["Alice", "Bob"])
        });

        SearchQuery result = accessorMock.Object.FromQueryString<SearchQuery>();

        Assert.IsNull(result.Name);
    }

    [TestMethod]
    public void FromQueryString_KeyMatchingIsCaseInsensitive()
    {
        Mock<IHttpContextAccessor> accessorMock = BuildAccessorMock(new Dictionary<string, StringValues>
        {
            ["name"] = "Alice",
            ["CATEGORY"] = "Books"
        });

        SearchQuery result = accessorMock.Object.FromQueryString<SearchQuery>();

        Assert.AreEqual("Alice", result.Name);
        Assert.AreEqual("Books", result.Category);
    }

    private static Mock<IHttpContextAccessor> BuildAccessorMock(Dictionary<string, StringValues> queryParams)
    {
        Mock<IHttpContextAccessor> accessorMock = new();
        Mock<IHttpContext> contextMock = new();
        Mock<IHttpRequest> requestMock = new();
        accessorMock.Setup(x => x.HttpContext).Returns(contextMock.Object);
        contextMock.SetupGet(x => x.Request).Returns(requestMock.Object);
        requestMock.SetupGet(x => x.Query).Returns(queryParams);
        return accessorMock;
    }
}

public class SearchQuery
{
    public string? Name { get; set; }
    public string? Category { get; set; }
}
