using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Moq;

using N2.Core.Commands;
using N2.Core.Exceptions;
using N2.Core.Identity;

using System.Security.Claims;
using System.Text;

namespace N2.Core.UnitTests.Identity;

[TestClass]
public class WithOAuthCommandHandler
{
    private Mock<ILogger> _logger = null!;
    private Mock<IRuntimeValidator<TokenRequest>> _validator = null!;
    private Mock<IIdentityManager> _identityManager = null!;
    private OAuthConfig _config = null!;

    public TestContext TestContext { get; set; } = null!;

    // 64-byte base64-encoded key, same format as JwtToolsTests
    private const string TestSecret = "QDebDB8kUuybqB5bgV6YI8R5bmM+u2AqhpfyGKCSdXtTdlIvzB8uhUEv2+wx4qZ+18Qx/dtu+WtXlNpd9yFkHQ==";
    private const string TotpSharedSecret = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";

    [TestInitialize]
    public void TestInitialize()
    {
        _logger = new Mock<ILogger>();
        _validator = new Mock<IRuntimeValidator<TokenRequest>>();
        _identityManager = new Mock<IIdentityManager>();

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OAuthConfig:Secret"] = TestSecret,
                ["OAuthConfig:Issuer"] = "http://test-issuer",
                ["OAuthConfig:TokenTimeoutInMinutes"] = "60"
            })
            .Build();

        _config = new OAuthConfig(configuration);
    }

    private OAuthCommandHandler CreateSut() => new(
        _logger.Object,
        _validator.Object,
        _config,
        _identityManager.Object);

    [TestMethod]
    public void CanInitialize()
    {
        OAuthCommandHandler sut = CreateSut();
        Assert.IsNotNull(sut);
    }

    [TestMethod]
    public async Task HandleRequestReturnsNotAcceptableForUnsupportedGrantType()
    {
        TokenRequest request = new()
        {
            Audience = "http://localhost",
            Value = new Token { GrantType = "unsupported_grant" }
        };

        OAuthCommandHandler sut = CreateSut();
        TokenResponse result = await sut.HandleRequest(request, TestContext.CancellationToken);

        Assert.AreEqual(ResponseStatus.NotAcceptable, result.Status);
    }

    [TestMethod]
    public async Task HandleRequestReturnsUnauthorizedWhenBasicAuthUserNotFound()
    {
        string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes("user:wrongpassword"));
        TokenRequest request = new()
        {
            Audience = "http://localhost",
            Value = new Token { GrantType = GrantType.Basic, AccessToken = credentials }
        };

        _identityManager
            .Setup(m => m.LogonUser("user", "wrongpassword", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.Empty);

        OAuthCommandHandler sut = CreateSut();
        TokenResponse result = await sut.HandleRequest(request, TestContext.CancellationToken);

        Assert.AreEqual(ResponseStatus.Unauthorized, result.Status);
    }

    [TestMethod]
    public async Task HandleRequestReturnsTokenForValidBasicAuth()
    {
        string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes("user:correctpassword"));
        Guid userSid = Guid.NewGuid();
        TokenRequest request = new()
        {
            Audience = "http://localhost",
            Value = new Token { GrantType = GrantType.Basic, AccessToken = credentials, Scope = "api" }
        };

        using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
        CancellationToken token = cts.Token;
        _identityManager.Setup(m => m.LogonUser("user", "correctpassword", token)).ReturnsAsync(userSid);
        _identityManager.Setup(m => m.GetClaims(userSid, "http://localhost", "api", token))
            .ReturnsAsync([new Claim(ClaimTypes.Name, "user")]);
        _identityManager.Setup(m => m.RegisterRefreshToken(userSid, "http://localhost", "api", It.IsAny<Guid>(), It.IsAny<DateTime>(), token))
            .Returns(Task.FromResult((ICommandResponse)StringResponse.Accept("")));

        OAuthCommandHandler sut = CreateSut();
        TokenResponse result = await sut.HandleRequest(request, token);

        Assert.AreEqual(ResponseStatus.Success, result.Status);
        Assert.IsNotNull(result.Value?.AccessToken);
        _identityManager.Verify(m => m.LogonUser("user", "correctpassword", token), Times.Once);
        _identityManager.Verify(m => m.GetClaims(userSid, "http://localhost", "api", token), Times.Once);
        _identityManager.Verify(m => m.RegisterRefreshToken(userSid, "http://localhost", "api", It.IsAny<Guid>(), It.IsAny<DateTime>(), token), Times.Once);
    }

    [TestMethod]
    public async Task HandleRequestReturnsUnauthorizedForBasicAuthWithNoColon()
    {
        // A valid Base64 string that decodes to a value with no colon
        string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes("usernameonly"));
        TokenRequest request = new()
        {
            Audience = "http://localhost",
            Value = new Token { GrantType = GrantType.Basic, AccessToken = credentials }
        };

        OAuthCommandHandler sut = CreateSut();
        await Assert.ThrowsAsync<UnauthorizedException>(() => sut.HandleRequest(request, TestContext.CancellationToken));

    }

    [TestMethod]
    public async Task HandleRequestReturnsUnauthorizedForBasicAuthWithEmptyUsername()
    {
        string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(":password"));
        TokenRequest request = new()
        {
            Audience = "http://localhost",
            Value = new Token { GrantType = GrantType.Basic, AccessToken = credentials }
        };

        OAuthCommandHandler sut = CreateSut();

        await Assert.ThrowsAsync<UnauthorizedException>(() => sut.HandleRequest(request, TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task HandleRequestSplitsBasicAuthAtFirstColonOnly()
    {
        // Password contains a colon — must not be truncated
        string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes("user:pass:word"));
        Guid userSid = Guid.NewGuid();
        TokenRequest request = new()
        {
            Audience = "http://localhost",
            Value = new Token { GrantType = GrantType.Basic, AccessToken = credentials, Scope = "api" }
        };

        _identityManager.Setup(m => m.LogonUser("user", "pass:word", It.IsAny<CancellationToken>())).ReturnsAsync(userSid);
        _identityManager.Setup(m => m.GetClaims(userSid, "http://localhost", "api", It.IsAny<CancellationToken>()))
            .ReturnsAsync([new Claim(ClaimTypes.Name, "user")]);
        _identityManager.Setup(m => m.RegisterRefreshToken(userSid, "http://localhost", "api", It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult((ICommandResponse)StringResponse.Accept("")));

        OAuthCommandHandler sut = CreateSut();
        TokenResponse result = await sut.HandleRequest(request, TestContext.CancellationToken);

        Assert.AreEqual(ResponseStatus.Success, result.Status);
    }

    [TestMethod]
    public async Task HandleRequestReturnsLogoffForLogoffGrantType()
    {
        Guid refreshToken = Guid.NewGuid();
        TokenRequest request = new()
        {
            Audience = "http://localhost",
            Value = new Token { GrantType = GrantType.Logoff, RefreshToken = refreshToken.ToString(), Scope = "api" }
        };

        using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
        CancellationToken token = cts.Token;
        _identityManager.Setup(m => m.LogoffUser(refreshToken, "api", token)).Returns(Task.FromResult((ICommandResponse)StringResponse.NoContent("")));

        OAuthCommandHandler sut = CreateSut();
        TokenResponse result = await sut.HandleRequest(request, token);
        _identityManager.Verify(m => m.LogoffUser(refreshToken, "api", token), Times.Once);

        Assert.AreEqual(ResponseStatus.Accepted, result.Status);
        Assert.AreEqual(GrantType.Logoff, result.Value?.TokenType);
    }

    [TestMethod]
    public async Task HandleRequestReturnsUnauthorizedWhenRefreshTokenNotFound()
    {
        TokenRequest request = new()
        {
            Audience = "http://localhost",
            Value = new Token { GrantType = GrantType.RefreshToken, RefreshToken = Guid.NewGuid().ToString() }
        };

        _identityManager
            .Setup(m => m.RefreshUserToken(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.Empty);

        OAuthCommandHandler sut = CreateSut();
        TokenResponse result = await sut.HandleRequest(request, TestContext.CancellationToken);

        Assert.AreEqual(ResponseStatus.Unauthorized, result.Status);
    }

    [TestMethod]
    public async Task HandleRequestReturnsTokenWhenRefreshTokenIsValid()
    {
        Guid userSid = Guid.NewGuid();
        Guid refreshToken = Guid.NewGuid();
        TokenRequest request = new()
        {
            Audience = "http://localhost",
            Value = new Token { GrantType = GrantType.RefreshToken, RefreshToken = refreshToken.ToString(), Scope = "api" }
        };

        _identityManager.Setup(m => m.RefreshUserToken(refreshToken, It.IsAny<CancellationToken>())).ReturnsAsync(userSid);
        _identityManager.Setup(m => m.GetClaims(userSid, "http://localhost", "api", It.IsAny<CancellationToken>()))
            .ReturnsAsync([new Claim(ClaimTypes.Name, "user")]);
        _identityManager.Setup(m => m.RegisterRefreshToken(userSid, "http://localhost", "api", It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult((ICommandResponse)StringResponse.Accept("")));

        OAuthCommandHandler sut = CreateSut();
        TokenResponse result = await sut.HandleRequest(request, TestContext.CancellationToken);

        Assert.AreEqual(ResponseStatus.Success, result.Status);
        Assert.IsNotNull(result.Value?.AccessToken);
    }

    [TestMethod]
    public async Task HandleClientCredentials_MissingClientId_ThrowsUnauthorizedException()
    {
        TokenRequest request = new()
        {
            Audience = "http://localhost",
            Value = new Token { GrantType = GrantType.ClientCredentials, ClientId = null, Scope = "api", ClientSecret = "secret" }
        };

        OAuthCommandHandler sut = CreateSut();
        await Assert.ThrowsAsync<UnauthorizedException>(() => sut.HandleRequest(request, TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task HandleClientCredentials_MissingScope_ThrowsUnauthorizedException()
    {
        TokenRequest request = new()
        {
            Audience = "http://localhost",
            Value = new Token { GrantType = GrantType.ClientCredentials, ClientId = "client1", Scope = null, ClientSecret = "secret" }
        };

        OAuthCommandHandler sut = CreateSut();
        await Assert.ThrowsAsync<UnauthorizedException>(() => sut.HandleRequest(request, TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task HandleClientCredentials_MissingClientSecret_ThrowsUnauthorizedException()
    {
        TokenRequest request = new()
        {
            Audience = "http://localhost",
            Value = new Token { GrantType = GrantType.ClientCredentials, ClientId = "client1", Scope = "api", ClientSecret = null }
        };

        OAuthCommandHandler sut = CreateSut();
        await Assert.ThrowsAsync<UnauthorizedException>(() => sut.HandleRequest(request, TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task HandleClientCredentials_EmptyStoredSecret_ThrowsUnauthorizedException()
    {
        _identityManager
            .Setup(m => m.GetUserSecret("client1", "api", It.IsAny<CancellationToken>()))
            .ReturnsAsync(StringResponse.Accept(""));

        TokenRequest request = new()
        {
            Audience = "http://localhost",
            Value = new Token { GrantType = GrantType.ClientCredentials, ClientId = "client1", Scope = "api", ClientSecret = "placeholder" }
        };

        OAuthCommandHandler sut = CreateSut();
        await Assert.ThrowsAsync<UnauthorizedException>(() => sut.HandleRequest(request, TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task HandleClientCredentials_InvalidTotp_ReturnsUnauthorized()
    {
        string wrongHash = JwtTools.TOTP(DateTime.UtcNow, TestSecret, OAuthConfig.DefaultReplayWindow);
        _identityManager
            .Setup(m => m.GetUserSecret("client1", "api", It.IsAny<CancellationToken>()))
            .ReturnsAsync(StringResponse.Accept(TotpSharedSecret));

        TokenRequest request = new()
        {
            Audience = "http://localhost",
            Value = new Token { GrantType = GrantType.ClientCredentials, ClientId = "client1", Scope = "api", ClientSecret = wrongHash }
        };

        OAuthCommandHandler sut = CreateSut();
        TokenResponse result = await sut.HandleRequest(request, TestContext.CancellationToken);

        Assert.AreEqual(ResponseStatus.Unauthorized, result.Status);
    }

    [TestMethod]
    public async Task HandleClientCredentials_ValidTotp_ReturnsToken()
    {
        string validHash = JwtTools.TOTP(DateTime.UtcNow, TotpSharedSecret, OAuthConfig.DefaultReplayWindow);
        Guid userSid = Guid.NewGuid();
        using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
        CancellationToken token = cts.Token;
        _identityManager.Setup(m => m.GetUserSecret("client1", "api", token)).ReturnsAsync(StringResponse.Accept(TotpSharedSecret));
        _identityManager.Setup(m => m.LogonUserWithSecret("client1", validHash, token)).ReturnsAsync(userSid);
        _identityManager.Setup(m => m.GetClaims(userSid, "http://localhost", "api", token))
            .ReturnsAsync([new Claim(ClaimTypes.Name, "client1")]);
        _identityManager.Setup(m => m.RegisterRefreshToken(userSid, "http://localhost", "api", It.IsAny<Guid>(), It.IsAny<DateTime>(), token))
            .Returns(Task.FromResult((ICommandResponse)StringResponse.Accept("")));

        TokenRequest request = new()
        {
            Audience = "http://localhost",
            Value = new Token { GrantType = GrantType.ClientCredentials, ClientId = "client1", Scope = "api", ClientSecret = validHash }
        };

        OAuthCommandHandler sut = CreateSut();
        TokenResponse result = await sut.HandleRequest(request, token);

        Assert.AreEqual(ResponseStatus.Success, result.Status);
        Assert.IsNotNull(result.Value?.AccessToken);
        _identityManager.Verify(m => m.GetUserSecret("client1", "api", token), Times.Once);
        _identityManager.Verify(m => m.LogonUserWithSecret("client1", validHash, token), Times.Once);
    }
}
