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
    }

    /// <summary>
    /// Gets or sets the secret.
    /// </summary>
    public string? Secret { get; set; }

    /// <summary>
    /// Gets or sets the issuer.
    /// </summary>
    public string? Issuer { get; set; }

    /// <summary>
    /// Gets or sets the token timeout in minutes.
    /// </summary>
    public long TokenTimeoutInMinutes { get; set; }
}
