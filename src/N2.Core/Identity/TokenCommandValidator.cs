using System.Diagnostics.CodeAnalysis;

using N2.Core.Exceptions;

namespace N2.Core.Identity;

/// <summary>
/// The token command validator.
/// </summary>
public class TokenRequestValidator : IRuntimeValidator<TokenRequest>
{
    /// <summary>
    /// Validates the TokenCommand.
    /// </summary>
    /// <param name="item">The item.</param>
    public void Validate([NotNull] TokenRequest item)
    {
        ArgumentNullException.ThrowIfNull(item);
        Token? content = item.Value;
        if (content == null)
        {
            throw new ConfigurationException("Token is missing from token requst.");
        }
        // check other things...
    }
}
