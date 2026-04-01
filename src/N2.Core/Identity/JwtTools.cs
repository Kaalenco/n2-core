using Microsoft.IdentityModel.Tokens;

using System.Diagnostics;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

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
        /// <param name="issuer">Expected issuer. When provided, the token's issuer is validated against this value.</param>
        /// <param name="audience">Expected audience. When provided, the token's audience is validated against this value.</param>
        /// <returns>A ClaimsPrincipal.</returns>
        public static ClaimsPrincipal GetPrincipal(string authorization, byte[] key, string? issuer = null, string? audience = null)
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

            return GetPrincipalFromJwt(bearer[1], key, issuer, audience);
        }


        /// <summary>
        /// Gets the principal from a jason web token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="key">The secret key, to validate the token.</param>
        /// <param name="issuer">Expected issuer. When provided, the token's issuer is validated against this value.</param>
        /// <param name="audience">Expected audience. When provided, the token's audience is validated against this value.</param>
        /// <returns>A ClaimsPrincipal.</returns>
        public static ClaimsPrincipal GetPrincipalFromJwt(string token, byte[] key, string? issuer = null, string? audience = null)
        {
            try
            {
                JwtSecurityTokenHandler tokenHandler = new();
                JwtSecurityToken jwtToken = (JwtSecurityToken)tokenHandler.ReadToken(token);
                if (jwtToken == null)
                {
                    return null!;
                }

                TokenValidationParameters parameters = new()
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = issuer != null,
                    ValidIssuer = issuer,
                    ValidateAudience = audience != null,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

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
        /// <param name="timeWindowInSeconds">The time window in seconds.</param>
        /// <returns>A bool.</returns>
        public static bool ValidateTOTP(DateTime referenceTime, string hash, string secret, int timeWindowInSeconds)
        {
            DateTime refTime = new(referenceTime.Year, referenceTime.Month, referenceTime.Day, referenceTime.Hour, referenceTime.Minute, 0);
            byte[] key = Convert.FromBase64String(secret);
            byte[] hashKey = Convert.FromBase64String(hash);
            long epoch = (long)(refTime - UnixEpoch).TotalSeconds / timeWindowInSeconds;

            var timer = Stopwatch.StartNew();
            bool matched;
            using (HMACSHA256 hmac = new(key))
            {
                matched = CheckHash(hashKey, hmac.ComputeHash(Encoding.UTF8.GetBytes(epoch.ToString(CultureInfo.InvariantCulture))))
                       || CheckHash(hashKey, hmac.ComputeHash(Encoding.UTF8.GetBytes((epoch + 1).ToString(CultureInfo.InvariantCulture))))
                       || CheckHash(hashKey, hmac.ComputeHash(Encoding.UTF8.GetBytes((epoch - 1).ToString(CultureInfo.InvariantCulture))));
            }
            // Enforce a minimum elapsed time once per validation call to resist
            // network-level timing analysis without blocking 3x on each inner check.
            long elapsed = timer.ElapsedMilliseconds;
            if (elapsed < 500)
            {
                Thread.Sleep((int)(500 - elapsed));
            }
            return matched;
        }

        /// <summary>
        /// Create a Time based One Time Password hash.
        /// </summary>
        /// <param name="referenceTime">The reference time.</param>
        /// <param name="secret">The secret.</param>
        /// <param name="timeWindowInSeconds">The time window in seconds.</param>
        /// <returns>A string.</returns>
        public static string TOTP(DateTime referenceTime, string secret, int timeWindowInSeconds)
        {
            DateTime refTime = new(referenceTime.Year, referenceTime.Month, referenceTime.Day, referenceTime.Hour, referenceTime.Minute, 0);
            byte[] key = Convert.FromBase64String(secret);
            long epoch = (long)(refTime - UnixEpoch).TotalSeconds / timeWindowInSeconds;
            using (HMACSHA256 hmac = new(key))
            {
                return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(epoch.ToString(CultureInfo.InvariantCulture))));
            }
        }

        /// <summary>
        /// Constant-time byte array comparison. Uses XOR accumulation so execution
        /// time does not vary with the position of the first differing byte,
        /// preventing timing side-channel attacks.
        /// The caller is responsible for enforcing a minimum wall-clock delay.
        /// </summary>
        private static bool CheckHash(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }
            int diff = 0;
            for (int i = 0; i < left.Length; i++)
            {
                diff |= left[i] ^ right[i];
            }
            return diff == 0;
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
