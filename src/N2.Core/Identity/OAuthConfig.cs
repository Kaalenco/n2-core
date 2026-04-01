using Microsoft.Extensions.Configuration;

namespace N2.Core.Identity;

/// <summary>
/// The oauth config.
/// </summary>
public class OAuthConfig
{
    /// <summary>
    /// The default token time out.
    /// </summary>
    public const long DefaultTokenTimeOut = 20;
    /// <summary>
    /// The default TOTP replay window in seconds (30 s).
    /// </summary>
    public const int DefaultReplayWindow = 30;

    /// <summary>
    /// Initializes a new instance of the <see cref="OAuthConfig"/> class.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    public OAuthConfig(IConfiguration configuration)
    {
        Contract.NotNull(configuration, nameof(configuration));
        Secret = configuration["OAuthConfig:Secret"];
        Issuer = configuration["OAuthConfig:Issuer"];
        string? timeout = configuration["OAuthConfig:TokenTimeoutInMinutes"];
        if (long.TryParse(timeout, out long timeoutMinutes))
        {
            TokenTimeoutInMinutes = timeoutMinutes;
        }
        else
        {
            TokenTimeoutInMinutes = DefaultTokenTimeOut;
        }
        string? replay = configuration["OAuthConfig:ReplayWindowInSeconds"];
        if (int.TryParse(replay, out int replaySeconds))
        {
            ReplayWindowInSeconds = replaySeconds;
        }
        else
        {
            ReplayWindowInSeconds = DefaultReplayWindow;
        }
    }

    /// <summary>
    /// Gets the current secret.
    /// </summary>
    public string? Secret { get; private set; }

    /// <summary>
    /// Gets the current issuer.
    /// </summary>
    public string? Issuer { get; private set; }

    /// <summary>
    /// Gets or sets the lifetime of an issued JWT in minutes.
    /// A bearer token is accepted until this many minutes after it was issued.
    /// Defaults to <see cref="DefaultTokenTimeOut"/>.
    /// </summary>
    public long TokenTimeoutInMinutes { get; set; }

    /// <summary>
    /// Gets or sets the TOTP time-slot size in seconds used for the
    /// <c>client_credentials</c> grant type.
    /// A TOTP hash generated in epoch <c>N</c> is accepted for epochs
    /// <c>N-1</c>, <c>N</c>, and <c>N+1</c>, giving an effective replay
    /// tolerance of ± one window around the generation time.
    /// Smaller values reduce the window in which a captured hash can be replayed.
    /// Defaults to <see cref="DefaultReplayWindow"/>.
    /// </summary>
    public int ReplayWindowInSeconds { get; set; }
}
