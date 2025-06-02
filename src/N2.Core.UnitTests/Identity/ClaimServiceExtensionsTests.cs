using N2.Core.Identity;

namespace OAuthServices
{
    /// <summary>
    /// Claim extensions tests
    /// </summary>
    [TestClass]
    public class ClaimServiceExtensionsTests
    {
        /// <summary>
        /// Validate that the claims service extensions translates claim type enum.
        /// </summary>
        /// <param name="claimType">The claim type.</param>
        /// <param name="expected">The expected result.</param>
        [DataTestMethod]
        [DataRow(ClaimType.Email, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")]
        public void ClaimServiceExtensionsTranslatesClaimTypeEnum(ClaimType claimType, string expected)
        {
            string result = ClaimServiceExtensions.ClaimName(claimType);
            Assert.AreEqual(expected, result);
        }
    }
}
