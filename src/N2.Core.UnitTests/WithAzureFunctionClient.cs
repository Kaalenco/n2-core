using System.Net;
using System.Text.Json;

using Moq;

using N2.Core.Http;

namespace N2.Core.UnitTests;

[TestClass]
public class WithAzureFunctionClient
{
    private Mock<IActivityLogger> _activityLogger = null!;
    private Mock<IDefaultValueService> _defaultValueService = null!;
    private Mock<IHttpClient> _httpClient = null!;
    private static readonly Uri BasePath = new("https://func.example.com");
    private const string ApiKey = "test-api-key";
    private static readonly JsonSerializerOptions SerializerOptions = new();

    [TestInitialize]
    public void TestInitialize()
    {
        _activityLogger = new Mock<IActivityLogger>();
        _defaultValueService = new Mock<IDefaultValueService>();
        _httpClient = new Mock<IHttpClient>();

        _defaultValueService
            .Setup(d => d.JsonSerializerOptions)
            .Returns(SerializerOptions);
    }

    private AzureFunctionClient CreateSut() => new(
        _activityLogger.Object,
        _defaultValueService.Object,
        _httpClient.Object,
        BasePath,
        ApiKey);

    [TestMethod]
    public void CanInitialize()
    {
        AzureFunctionClient sut = CreateSut();
        Assert.IsNotNull(sut);
    }

    [TestMethod]
    public async Task CallAsyncReturnsDefaultWhenResponseIsNotSuccess()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        _httpClient
            .Setup(h => h.PostAsync(It.IsAny<Uri>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        string? result = await CreateSut().CallAsync<object, string>("test", new { }, CancellationToken.None);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task CallAsyncReturnsDefaultWhenResponseBodyIsEmpty()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(string.Empty)
        };
        _httpClient
            .Setup(h => h.PostAsync(It.IsAny<Uri>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        string? result = await CreateSut().CallAsync<object, string>("test", new { }, CancellationToken.None);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task CallAsyncReturnsStringWhenResponseTypeIsString()
    {
        const string responseBody = "hello world";
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseBody)
        };
        _httpClient
            .Setup(h => h.PostAsync(It.IsAny<Uri>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
   

        string? result = await CreateSut().CallAsync<object, string>("test", new { }, CancellationToken.None);

        Assert.AreEqual(responseBody, result);
    }

    [TestMethod]
    public async Task CallAsyncDeserializesResponseBodyWhenResponseTypeIsObject()
    {
        TestPayload payload = new() { Name = "n2", Value = 42 };
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, SerializerOptions))
        };

        _httpClient
            .Setup(h => h.PostAsync(It.IsAny<Uri>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        TestPayload? result = await CreateSut().CallAsync<object, TestPayload>("test", new { }, CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.AreEqual(payload.Name, result.Name);
        Assert.AreEqual(payload.Value, result.Value);
    }

    [TestMethod]
    public async Task CallAsyncSendsApiKeyHeader()
    {
        HttpRequestMessage? captured = null;
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("response")
        };
        _httpClient
            .Setup(h => h.PostAsync(It.IsAny<Uri>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .Callback<Uri, HttpContent, CancellationToken>((_, content, _) =>
            {
                captured = new HttpRequestMessage { Content = content };
            })
            .ReturnsAsync(response);

        await CreateSut().CallAsync<object, string>("fn", new { }, CancellationToken.None);

        _httpClient.Verify(h => h.PostAsync(
            It.Is<Uri>(u => u.ToString().Contains("/api/fn")),
            It.IsAny<HttpContent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private sealed class TestPayload
    {
        public string? Name { get; set; }
        public int Value { get; set; }
    }
}
