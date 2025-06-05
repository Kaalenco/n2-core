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
    public void Validate(TokenRequest item)
    {
        Contract.NotNull(item, nameof(item));
        Token? content = item.Value;
        if (content == null)
        {
            throw new ConfigurationException("Token is missing from token requst.");
        }
        // check other things...
    }
}
