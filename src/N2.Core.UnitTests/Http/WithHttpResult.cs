using System.Net;
using System.Text.Json;

using Moq;

using N2.Core.Http;

namespace N2.Core.UnitTests.Http;

[TestClass]
public class WithHttpResult
{
    [TestMethod]
    public void IsSuccessStatusCodeLessThan300ReturnsTrue()
    {
        // Arrange
        HttpResult result = new() { StatusCode = HttpStatusCode.OK };

        // Act
        bool isSuccess = result.IsSuccess();

        // Assert
        Assert.IsTrue(isSuccess);
    }

    [TestMethod]
    public void IsSuccessStatusCodeGreaterThan300ReturnsFalse()
    {
        // Arrange
        HttpResult result = new() { StatusCode = HttpStatusCode.BadRequest };

        // Act
        bool isSuccess = result.IsSuccess();

        // Assert
        Assert.IsFalse(isSuccess);
    }

    [TestMethod]
    public void OkStaticReturnsOkStatusCode()
    {
        // Act
        HttpResult result = HttpResult.Ok();

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    [TestMethod]
    public void NotFoundStaticReturnsNotFoundStatusCode()
    {
        // Act
        HttpResult result = HttpResult.NotFound();

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
    }

    [TestMethod]
    public void NoContentStaticReturnsNoContentStatusCode()
    {
        // Act
        HttpResult result = HttpResult.NoContent();

        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
    }

    [TestMethod]
    public void NotAcceptedStaticReturnsNotAcceptableStatusCode()
    {
        // Act
        HttpResult result = HttpResult.NotAccepted();

        // Assert
        Assert.AreEqual(HttpStatusCode.NotAcceptable, result.StatusCode);
    }

    [TestMethod]
    public void OkWithMessageSetsMessageAndStatusCode()
    {
        // Arrange
        string message = "Test message";

        // Act
        HttpResult result = HttpResult.Ok(message);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        Assert.AreEqual(message, result.Message);
    }

    [TestMethod]
    public void OkTWithResultSetsResultAndStatusCode()
    {
        // Arrange
        var testObject = new { Name = "Test" };

        // Act
        var result = HttpResult.Ok(testObject);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        Assert.AreEqual(testObject, result.Result);
    }

    [TestMethod]
    public void CreatedWithResultSetsResultAndStatusCode()
    {
        // Arrange
        var testObject = new { Name = "Test" };
        string message = "Created successfully";

        // Act
        var result = HttpResult.Created(testObject, message);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
        Assert.AreEqual(testObject, result.Result);
        Assert.AreEqual(message, result.Message);
    }

    [TestMethod]
    public void ExceptionSetsInternalErrorAndMessageFromException()
    {
        // Arrange
        Exception exception = new ArgumentException("Test exception");

        // Act
        HttpResult result = HttpResult.Exception(exception);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
        Assert.AreEqual(exception.Message, result.Message);
    }

    [TestMethod]
    public async Task WriteJsonResponseSetsHttpHeaders()
    {
        // Arrange
        Mock<IHttpContext> httpContextMock = new();
        Mock<IHttpResponse> httpResponseMock = new();
        Dictionary<string, string> headers = new();

        httpContextMock.SetupGet(c => c.Response).Returns(httpResponseMock.Object);
        httpResponseMock.Setup(c => c.SetHeader(It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string>((key, value) => headers[key] = value);

        string testData = "Test data";
        HttpResult<string> result = HttpResult.Ok(testData, "Test message");
        result.Etag = "W/\"123\"";
        result.ReferenceUri = new Uri("https://example.com/api/resource/123");

        JsonSerializerOptions options = new();
        CancellationToken token = CancellationToken.None;

        IHttpContext context = httpContextMock.Object;
        // Act
        await result.WriteJsonResponse(context, options, token);

        // Assert
        Assert.AreEqual("application/json", headers["Content-Type"].ToString());
        Assert.AreEqual(typeof(string).FullName, headers["X-Type"].ToString());
        Assert.AreEqual("Test message", headers["X-Message"].ToString());
        Assert.AreEqual("W/\"123\"", headers["X-AuditTag"].ToString());
        Assert.AreEqual("https://example.com/api/resource/123", headers["X-ReferenceUri"].ToString());

        // Verify WriteAsJsonAsync was called
        httpResponseMock.Verify(
            r => r.WriteAsJsonAsync(testData, options, token),
            Times.Once);
    }
}