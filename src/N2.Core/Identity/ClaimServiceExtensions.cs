
namespace N2.Core.Identity
{
    /// <summary>
    /// The claim service extensions.
    /// </summary>
    public static class ClaimServiceExtensions
    {
        /// <summary>
        /// Dictionary to translate claimtype to its string equivalent.
        /// </summary>
        private static readonly Dictionary<ClaimType, Func<string>> _claims = new()
        {
            { ClaimType.UserName, () => System.Security.Claims.ClaimTypes.Name },
            { ClaimType.Email, () => System.Security.Claims.ClaimTypes.Email },
            { ClaimType.Actor, () => System.Security.Claims.ClaimTypes.Actor },
            { ClaimType.Role, () => System.Security.Claims.ClaimTypes.Role },
            { ClaimType.GivenName, () => System.Security.Claims.ClaimTypes.GivenName },
            { ClaimType.PrimarySid, () => System.Security.Claims.ClaimTypes.PrimarySid },
            { ClaimType.Culture, () => System.Security.Claims.ClaimTypes.Locality },
        };

        /// <summary>
        /// The uri for the claim.
        /// </summary>
        /// <param name="claimType">Type of claim</param>
        /// <returns></returns>
        public static string ClaimName(this ClaimType claimType)
        {
            if (_claims.TryGetValue(claimType, out Func<string>? value))
            {
                return value.Invoke();
            }

            return $"http://localhost/undefined-claims/{claimType}";
        }

        /// <summary>
        /// Convert a date time value to the equivalent unix time.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>A long.</returns>
        public static long ToUnixTime(this DateTime dateTime)
        {
            DateTime dateTimeUtc = dateTime.ToUniversalTime();
            return (long)(dateTimeUtc - dateTimeUtc.UnixEpoch()).TotalSeconds;
        }

        /// <summary>
        /// Get the Unix epoch date time (1970-01-01T00:00:00Z).
        /// </summary>
        /// <param name="dateTime">Parameter is only used for the extension.</param>
        /// <returns>A valid UTC time</returns>
        public static DateTime UnixEpoch(this DateTime dateTime) => new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }
}
