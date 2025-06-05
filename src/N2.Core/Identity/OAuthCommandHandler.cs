using System.Text;

using Microsoft.Extensions.Logging;

using N2.Core.Commands;
using N2.Core.Exceptions;

namespace N2.Core.Identity;

/// <summary>
/// The OAuth command handler.
/// </summary>
public class OAuthCommandHandler : BaseCommandHandler<TokenRequest, TokenResponse>
{
    private readonly IIdentityManager _identityManager;
    private readonly OAuthConfig _config;
    private readonly byte[] _secret;

    /// <summary>
    /// Initializes a new instance of the <see cref="OAuthCommandHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="config"> The OAuth configuration.</param>
    /// <param name="identityManager"> The identity manager.</param>
    /// <param name="validator">The validator.</param>
    public OAuthCommandHandler(
        ILogger logger,
        IRuntimeValidator<TokenRequest> validator,
        OAuthConfig config,
        IIdentityManager identityManager
        ) : base(logger, validator)
    {
        Contract.NotNull(config, nameof(config));
        Contract.NotNull(config.Secret, nameof(config.Secret));
        Contract.NotNull(config.Issuer, nameof(config.Issuer));

        _config = config;
        _secret = Convert.FromBase64String(config.Secret!);
        _identityManager = identityManager;
    }

    /// <summary>
    /// Handles the request async.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>A Task.</returns>
    public override async Task<TokenResponse> HandleRequestAsync(TokenRequest request)
    {
        Guid sid;
        Token? token = null;

        Contract.NotNull(request, nameof(request));
        token = request.Value;
        Contract.NotNull(token, nameof(token));
        // accept basic
        Dictionary<string, Func<Token, Task<Guid>>> handlers = new()
        {
            { "basic",  HandleBasicAuthentication },
            { "client_credentials" , HandleClientCredentials },
            { "access_token", HandleRefresh },
            { "refresh_token", HandleRefresh },
            { "refresh",  HandleRefresh },
            { "logoff_user",  HandleLogoff },
            { "logoff",  HandleLogoff }
        };

        if (handlers.TryGetValue(token!.GrantType, out Func<Token, Task<Guid>>? value))
        {
            sid = await value.Invoke(token);
        }
        else
        {
            return TokenResponse.Failed(ResponseStatus.NotAcceptable,
                $"authorization schema not supported: {token.GrantType}");
        }

        if (token.GrantType == GrantType.LogoffUser || token.GrantType == GrantType.Logoff)
        {
            return new TokenResponse(
                new Token
                {
                    AccessToken = null,
                    ExpiresIn = DateTime.UtcNow.ToUnixTime(),
                    Scope = token.Scope,
                    TokenType = GrantType.Logoff,
                    RefreshToken = string.Empty,
                },
                ResponseStatus.Accepted);
        }

        if (sid == Guid.Empty)
        {
            return TokenResponse.Failed(ResponseStatus.Unauthorized, "Username or password is invalid");
        }

        Token responseToken = await CreateValidJwt(sid, request.Audience, token.Scope ?? "*");

        return new TokenResponse(responseToken);
    }

    private async Task<Guid> HandleLogoff(Token token)
    {
        if (Guid.TryParse(token.RefreshToken, out Guid refresh))
        {
            await _identityManager.LogoffUser(refresh, token.Scope ?? "*");
        }
        return Guid.Empty;
    }

    /// <summary>
    /// Create a new token with a valid jwt access token
    /// </summary>
    /// <param name="sid">The security id for the user.</param>
    /// <param name="audience">The audience for the token use.</param>
    /// <param name="scope">The scope for the token claims.</param>
    /// <returns></returns>
    private async Task<Token> CreateValidJwt(Guid sid, string audience, string scope)
    {
        Guid refreshToken = Guid.NewGuid();
        DateTime tokenTimeOut = DateTime.UtcNow.AddMinutes(_config.TokenTimeoutInMinutes);
        Task refreshTask = _identityManager.RegisterRefreshToken(sid, audience, scope, refreshToken, tokenTimeOut);
        System.Security.Claims.Claim[] claims = await _identityManager.GetClaims(sid, audience, scope);

        string newToken = JwtTools.ConvertToJwt(claims, audience, _config.Issuer, _secret, tokenTimeOut);

        if (!refreshTask.Wait(200))
        {
            refreshToken = Guid.Empty;
        }

        return new Token
        {
            AccessToken = newToken,
            ExpiresIn = tokenTimeOut.ToUnixTime(),
            Scope = scope,
            TokenType = GrantType.AccessToken,
            RefreshToken = refreshToken.ToString(),
        };
    }

    private async Task<Guid> HandleBasicAuthentication(Token token)
    {
        if (string.IsNullOrEmpty(token.AccessToken))
        {
            throw new UnauthorizedException("No Accesstoken");
        }
        string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(token.AccessToken));
        string[] user = decoded.Split(':');
        return await _identityManager.LogonUser(user[0], user[1]);
    }

    private async Task<Guid> HandleRefresh(Token token)
    {
        if (Guid.TryParse(token.RefreshToken, out Guid refresh))
        {
            return await _identityManager.RefreshUserToken(refresh);
        }
        else
        {
            return Guid.Empty;
        }
    }

    private async Task<Guid> HandleClientCredentials(Token token)
    {
        if (string.IsNullOrEmpty(token.ClientId) || string.IsNullOrEmpty(token.Scope) || string.IsNullOrEmpty(token.ClientSecret))
        {
            throw new UnauthorizedException("No Credentials");
        }

        string secret = await _identityManager.GetUserSecret(token.ClientId!, token.Scope!);
        if (string.IsNullOrEmpty(secret))
        {
            throw new UnauthorizedException("No Access");
        }

        if (JwtTools.ValidateTOTP(DateTime.UtcNow, secret, token.ClientSecret!, 10))
        {
            return await _identityManager.LogonUserWithSecret(token.ClientId!, token.ClientSecret!);
        }
        else
        {
            return Guid.Empty;
        }
    }
}
