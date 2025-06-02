using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Microsoft.IdentityModel.Tokens;

namespace N2.Core.Identity
{
    /// <summary>
    /// The token timeout preset values.
    /// </summary>
    public static class JwtTokenTimeout
    {
        public const long FiveMinutes = 5;
        public const long OneHour = 60;
        public const long OneDay = OneHour * 24;
        public const long OneWeek = OneDay * 7;
        public const long SixWeeks = OneWeek * 6;
        public const long OneYear = OneWeek * 52;
    }

    /// <summary>
    /// Jason Web Token tools.
    /// </summary>
    public static class JwtTools
    {
        /// <summary>
        /// Creates a new random key string.
        /// </summary>
        /// <returns>A string.</returns>
        public static string CreateRandomKeyString()
        {
            return Convert.ToBase64String(CreateRandomKey());
        }

        /// <summary>
        /// Creates the random key.
        /// </summary>
        /// <returns>An array of byte.</returns>
        public static byte[] CreateRandomKey()
        {
            byte[] randomKey = new byte[64];
            using (RandomNumberGenerator randomGen = RandomNumberGenerator.Create())
            {
                randomGen.GetBytes(randomKey);
            }
            return randomKey;
        }

        /// <summary>
        /// Extract the principal from an HTTP request.
        /// </summary>
        /// <param name="authorization">The authorization request.</param>
        /// <param name="key">The key.</param>
        /// <returns>A ClaimsPrincipal.</returns>
        public static ClaimsPrincipal GetPrincipal(string authorization, byte[] key)
        {
            if (string.IsNullOrEmpty(authorization))
            {
                return new ClaimsPrincipal();
            }

            string[] bearer = authorization.Split(' ');
            if (!string.Equals("bearer", bearer[0], StringComparison.OrdinalIgnoreCase))
            {
                return new ClaimsPrincipal();
            }

            return GetPrincipalFromJwt(bearer[1], key);
        }


        /// <summary>
        /// Gets the principal from a jason web token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="key">The secret key, to validate the token.</param>
        /// <returns>A ClaimsPrincipal.</returns>
        public static ClaimsPrincipal GetPrincipalFromJwt(string token, byte[] key)
        {
            try
            {
                JwtSecurityTokenHandler tokenHandler = new();
                JwtSecurityToken jwtToken = (JwtSecurityToken)tokenHandler.ReadToken(token);
                if (jwtToken == null)
                {
                    return null!;
                }

#pragma warning disable CA5404 // Do not disable token validation checks
                TokenValidationParameters parameters = new()
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
#pragma warning restore CA5404 // Do not disable token validation checks

                ClaimsPrincipal principal = tokenHandler.ValidateToken(
                    token,
                    parameters,
                    out SecurityToken securityToken);
                return principal;
            }
            catch (Exception e)
            {
                throw new WebTokenException("JWT Token validation failed", e);
            }
        }

        /// <summary>
        /// The unix zero time epoch.
        /// </summary>
        public static DateTime UnixEpoch => new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Validates the Time based One Time Password hash.
        /// </summary>
        /// <param name="referenceTime">The reference time.</param>
        /// <param name="hash">The hash.</param>
        /// <param name="secret">The secret.</param>
        /// <param name="timeWindowInMinutes">The time window in minutes.</param>
        /// <returns>A bool.</returns>
        public static bool ValidateTOTP(DateTime referenceTime, string hash, string secret, int timeWindowInMinutes)
        {
            DateTime refTime = new(referenceTime.Year, referenceTime.Month, referenceTime.Day, referenceTime.Hour, referenceTime.Minute, 0);
            byte[] key = Convert.FromBase64String(secret);
            byte[] hashKey = Convert.FromBase64String(hash);
            long epoch = (long)Math.Round(((refTime - UnixEpoch).TotalSeconds + 30) / 60) / timeWindowInMinutes;

            using (HMACSHA256 hmac = new(key))
            {
                if (CheckHash(hashKey, hmac.ComputeHash(Encoding.UTF8.GetBytes(epoch.ToString(CultureInfo.InvariantCulture)))))
                {
                    return true;
                }
                // retry with extension earlier or later
                if (CheckHash(hashKey, hmac.ComputeHash(Encoding.UTF8.GetBytes((epoch + 1).ToString(CultureInfo.InvariantCulture)))))
                {
                    return true;
                }

                if (CheckHash(hashKey, hmac.ComputeHash(Encoding.UTF8.GetBytes((epoch - 1).ToString(CultureInfo.InvariantCulture)))))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Create a Time based One Time Password hash.
        /// </summary>
        /// <param name="referenceTime">The reference time.</param>
        /// <param name="secret">The secret.</param>
        /// <param name="timeWindowInMinutes">The time window in minutes.</param>
        /// <returns>A string.</returns>
        public static string TOTP(DateTime referenceTime, string secret, int timeWindowInMinutes)
        {
            DateTime refTime = new(referenceTime.Year, referenceTime.Month, referenceTime.Day, referenceTime.Hour, referenceTime.Minute, 0);
            byte[] key = Convert.FromBase64String(secret);
            long epoch = (long)Math.Round((refTime - UnixEpoch).TotalSeconds / 60) / timeWindowInMinutes;
            using (HMACSHA256 hmac = new(key))
            {
                return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(epoch.ToString(CultureInfo.InvariantCulture))));
            }
        }

        private static bool CheckHash(this byte[] left, byte[] right)
        {
            if (left == null || right == null)
            {
                return false;
            }

            if (left.Length != right.Length)
            {
                return false;
            }

            for (int i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Convert a claimset to a JSON Web Token.
        /// </summary>
        /// <param name="claims">The claims.</param>
        /// <param name="audience">The Intended audience for this token</param>
        /// <param name="tokenIssuer">The token issuer. Read this from the policy, under [edit] issuer claim, or use the iis claim if you have a previous login.</param>
        /// <param name="securityKey">The security key that is used to sign the JWT (key is .</param>
        /// <param name="tokenTimeout">The utc time that the token will time out.</param>
        /// <returns>System.String.</returns>
        public static string ConvertToJwt(
            this IEnumerable<Claim> claims,
            string? audience,
            string? tokenIssuer,
            byte[] securityKey,
            DateTime tokenTimeout)
        {
            Contract.NotNull(securityKey, nameof(securityKey));
            Contract.NotNull(tokenIssuer, nameof(tokenIssuer));
            Contract.NotNull(audience, nameof(audience));

            DateTime utcTime = tokenTimeout.ToUniversalTime();

            if (claims == null)
            {
                return string.Empty;
            }

            JwtSecurityToken token = new(
                issuer: tokenIssuer,
                audience: audience,
                claims: claims,
                expires: utcTime,
                notBefore: DateTime.UtcNow,
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(securityKey),
                    SecurityAlgorithms.HmacSha256
                )
            );
            JwtSecurityTokenHandler tokenHandler = new();
            return tokenHandler.WriteToken(token);
        }
    }
}
