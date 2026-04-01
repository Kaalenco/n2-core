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

    // 64-byte base64-encoded key, same format as JwtToolsTests
    private const string TestSecret = "QDebDB8kUuybqB5bgV6YI8R5bmM+u2AqhpfyGKCSdXtTdlIvzB8uhUEv2+wx4qZ+18Qx/dtu+WtXlNpd9yFkHQ==";

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
    public async Task HandleRequestAsyncReturnsNotAcceptableForUnsupportedGrantType()
    {
        TokenRequest request = new()
        {
            Audience = "http://localhost",
            Value = new Token { GrantType = "unsupported_grant" }
        };

        OAuthCommandHandler sut = CreateSut();
        TokenResponse result = await sut.HandleRequestAsync(request);

        Assert.AreEqual(ResponseStatus.NotAcceptable, result.Status);
    }

    [TestMethod]
    public async Task HandleRequestAsyncReturnsUnauthorizedWhenBasicAuthUserNotFound()
    {
        string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes("user:wrongpassword"));
        TokenRequest request = new()
        {
            Audience = "http://localhost",
            Value = new Token { GrantType = GrantType.Basic, AccessToken = credentials }
        };

        _identityManager
            .Setup(m => m.LogonUser("user", "wrongpassword"))
            .ReturnsAsync(Guid.Empty);

        OAuthCommandHandler sut = CreateSut();
        TokenResponse result = await sut.HandleRequestAsync(request);

        Assert.AreEqual(ResponseStatus.Unauthorized, result.Status);
    }

    [TestMethod]
    public async Task HandleRequestAsyncReturnsTokenForValidBasicAuth()
    {
        string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes("user:correctpassword"));
        Guid userSid = Guid.NewGuid();
        TokenRequest request = new()
        {
            Audience = "http://localhost",
            Value = new Token { GrantType = GrantType.Basic, AccessToken = credentials, Scope = "api" }
        };

        _identityManager.Setup(m => m.LogonUser("user", "correctpassword")).ReturnsAsync(userSid);
        _identityManager.Setup(m => m.GetClaims(userSid, "http://localhost", "api"))
            .ReturnsAsync(new[] { new Claim(ClaimTypes.Name, "user") });
        _identityManager.Setup(m => m.RegisterRefreshToken(userSid, "http://localhost", "api", It.IsAny<Guid>(), It.IsAny<DateTime>()))
            .Returns(Task.CompletedTask);

        OAuthCommandHandler sut = CreateSut();
        TokenResponse result = await sut.HandleRequestAsync(request);

        Assert.AreEqual(ResponseStatus.Success, result.Status);
        Assert.IsNotNull(result.Value?.AccessToken);
    }

    [TestMethod]
    public async Task HandleRequestAsyncReturnsUnauthorizedForBasicAuthWithNoColon()
    {
        // A valid Base64 string that decodes to a value with no colon
        string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes("usernameonly"));
        TokenRequest request = new()
        {
            Audience = "http://localhost",
            Value = new Token { GrantType = GrantType.Basic, AccessToken = credentials }
        };

        OAuthCommandHandler sut = CreateSut();
        await Assert.ThrowsAsync<UnauthorizedException>(() => sut.HandleRequestAsync(request));

    }

    [TestMethod]
    public async Task HandleRequestAsyncReturnsUnauthorizedForBasicAuthWithEmptyUsername()
    {
        string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(":password"));
        TokenRequest request = new()
        {
            Audience = "http://localhost",
            Value = new Token { GrantType = GrantType.Basic, AccessToken = credentials }
        };

        OAuthCommandHandler sut = CreateSut();

        await Assert.ThrowsAsync<UnauthorizedException>(() => sut.HandleRequestAsync(request));
    }

    [TestMethod]
    public async Task HandleRequestAsyncSplitsBasicAuthAtFirstColonOnly()
    {
        // Password contains a colon — must not be truncated
        string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes("user:pass:word"));
        Guid userSid = Guid.NewGuid();
        TokenRequest request = new()
        {
            Audience = "http://localhost",
            Value = new Token { GrantType = GrantType.Basic, AccessToken = credentials, Scope = "api" }
        };

        _identityManager.Setup(m => m.LogonUser("user", "pass:word")).ReturnsAsync(userSid);
        _identityManager.Setup(m => m.GetClaims(userSid, "http://localhost", "api"))
            .ReturnsAsync(new[] { new Claim(ClaimTypes.Name, "user") });
        _identityManager.Setup(m => m.RegisterRefreshToken(userSid, "http://localhost", "api", It.IsAny<Guid>(), It.IsAny<DateTime>()))
            .Returns(Task.CompletedTask);

        OAuthCommandHandler sut = CreateSut();
        TokenResponse result = await sut.HandleRequestAsync(request);

        Assert.AreEqual(ResponseStatus.Success, result.Status);
    }

    [TestMethod]
    public async Task HandleRequestAsyncReturnsLogoffForLogoffGrantType()
    {
        Guid refreshToken = Guid.NewGuid();
        TokenRequest request = new()
        {
            Audience = "http://localhost",
            Value = new Token { GrantType = GrantType.Logoff, RefreshToken = refreshToken.ToString(), Scope = "api" }
        };

        _identityManager.Setup(m => m.LogoffUser(refreshToken, "api")).Returns(Task.CompletedTask);

        OAuthCommandHandler sut = CreateSut();
        TokenResponse result = await sut.HandleRequestAsync(request);

        Assert.AreEqual(ResponseStatus.Accepted, result.Status);
        Assert.AreEqual(GrantType.Logoff, result.Value?.TokenType);
    }

    [TestMethod]
    public async Task HandleRequestAsyncReturnsUnauthorizedWhenRefreshTokenNotFound()
    {
        TokenRequest request = new()
        {
            Audience = "http://localhost",
            Value = new Token { GrantType = GrantType.RefreshToken, RefreshToken = Guid.NewGuid().ToString() }
        };

        _identityManager
            .Setup(m => m.RefreshUserToken(It.IsAny<Guid>()))
            .ReturnsAsync(Guid.Empty);

        OAuthCommandHandler sut = CreateSut();
        TokenResponse result = await sut.HandleRequestAsync(request);

        Assert.AreEqual(ResponseStatus.Unauthorized, result.Status);
    }

    [TestMethod]
    public async Task HandleRequestAsyncReturnsTokenWhenRefreshTokenIsValid()
    {
        Guid userSid = Guid.NewGuid();
        Guid refreshToken = Guid.NewGuid();
        TokenRequest request = new()
        {
            Audience = "http://localhost",
            Value = new Token { GrantType = GrantType.RefreshToken, RefreshToken = refreshToken.ToString(), Scope = "api" }
        };

        _identityManager.Setup(m => m.RefreshUserToken(refreshToken)).ReturnsAsync(userSid);
        _identityManager.Setup(m => m.GetClaims(userSid, "http://localhost", "api"))
            .ReturnsAsync(new[] { new Claim(ClaimTypes.Name, "user") });
        _identityManager.Setup(m => m.RegisterRefreshToken(userSid, "http://localhost", "api", It.IsAny<Guid>(), It.IsAny<DateTime>()))
            .Returns(Task.CompletedTask);

        OAuthCommandHandler sut = CreateSut();
        TokenResponse result = await sut.HandleRequestAsync(request);

        Assert.AreEqual(ResponseStatus.Success, result.Status);
        Assert.IsNotNull(result.Value?.AccessToken);
    }
}
