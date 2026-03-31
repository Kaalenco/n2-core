using System.Security.Claims;

using N2.Core.Identity;

namespace OAuthServices;
/// <summary>
/// The jwt tools tests.
/// </summary>

[TestClass]
public class JwtToolsTests
{
    /// <summary>
    /// Jwt tools can generate random keys.
    /// </summary>
    [TestMethod]
    public void JwtToolsCanGenerateRandomKeys()
    {
        string result = JwtTools.CreateRandomKeyString();
        Assert.IsNotNull(result);
        string result2 = JwtTools.CreateRandomKeyString();
        Assert.AreNotEqual(result, result2);
        Console.WriteLine($"Key 1:{result}");
        Console.WriteLine($"Key 2:{result2}");
    }

    /// <summary>
    /// Jwts the tools can create time based Password.
    /// </summary>
    [TestMethod]
    public void JwtToolsCanCreateTimeBasedPassword()
    {
        string secret = "QDebDB8kUuybqB5bgV6YI8R5bmM+u2AqhpfyGKCSdXtTdlIvzB8uhUEv2+wx4qZ+18Qx/dtu+WtXlNpd9yFkHQ==";
        DateTime datetime = new(2021, 12, 23, 10, 23, 23, DateTimeKind.Utc);
        string hash = JwtTools.TOTP(datetime, secret, 10);
        Assert.IsNotNull(hash);
        Console.WriteLine($"TOTP :{hash}");
    }

    /// <summary>
    /// Jwts the tools can validate time based password.
    /// </summary>
    [TestMethod]
    public void JwtToolsCanValidateTimeBasedPassword()
    {
        string secret = "QDebDB8kUuybqB5bgV6YI8R5bmM+u2AqhpfyGKCSdXtTdlIvzB8uhUEv2+wx4qZ+18Qx/dtu+WtXlNpd9yFkHQ==";
        string otherSecret = "o8Ql4xAzMnVf0xaSW7f+rgLkvyAVdF+OacsaG09qfzplSY912gToi4XUwltG0X9Dl4YlM9VwFYTNwj30ydUsJQ==";

        DateTime datetime = new(2021, 12, 23, 10, 23, 23, DateTimeKind.Utc);
        string hash = JwtTools.TOTP(datetime, secret, 10);
        Assert.IsNotNull(hash);

        // valid if time is the same
        Assert.IsTrue(JwtTools.ValidateTOTP(datetime, hash, secret, 10));
        // valid if time is within the window
        Assert.IsTrue(JwtTools.ValidateTOTP(datetime.AddMinutes(15), hash, secret, 10));
        Assert.IsTrue(JwtTools.ValidateTOTP(datetime.AddMinutes(-14), hash, secret, 10));

        // invalid if time is outside the window
        Assert.IsFalse(JwtTools.ValidateTOTP(datetime.AddMinutes(16), hash, secret, 10));
        Assert.IsFalse(JwtTools.ValidateTOTP(datetime.AddMinutes(-15), hash, secret, 10));

        // invalid is timeslot is different
        Assert.IsFalse(JwtTools.ValidateTOTP(datetime, hash, secret, 9));
        // invalid if secret is different
        Assert.IsFalse(JwtTools.ValidateTOTP(datetime, hash, otherSecret, 10));
    }

    /// <summary>
    /// Jwt tools can get principal from http request.
    /// </summary>
    [TestMethod]
    public void JwtToolsWillExpire()
    {
        string authorization = "bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3ByaW1hcnlzaWQiOiJjMjc0NDZhYi04ZjEzLTQ3YmItYTc2OS04ODA5MDE4MzVhOGMiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJ0ZXN0dXNlciIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJUZXN0VXNlciIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6InVzZXJAbXljb21wYW55LmxvY2FsIiwibmJmIjoxNjQwNDI5MTI2LCJleHAiOjE2NDA0Mjk3MjYsImlzcyI6Imh0dHA6Ly9Kd3RUb29sc1Rlc3RzIiwiYXVkIjoiaHR0cDovL2xvY2FsaG9zdCJ9.E0woMYw497uwUPG9UUTGw0t17GacSziDM32LVLgmF9Y";

        byte[] key = Convert.FromBase64String("OmuURxFCm3Vu4zXx8IqtHbhY8fsz9YtF++NDaD4rwj+nfqBU/ehaXbXMIjqAf51w3dnAFDmjWblW0EP6EuyE/w==");

        Assert.Throws<WebTokenException>(() =>
        {
            // Token is expired
            _ = JwtTools.GetPrincipal(authorization, key);
        });
    }

    /// <summary>
    /// Jwt tools can get principal from null request.
    /// </summary>
    [TestMethod]
    public void JwtToolsCanGetPrincipalFromNullRequest()
    {
        byte[] key = new byte[] { 0 };
        ClaimsPrincipal result = JwtTools.GetPrincipal(null!, key);
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Jwt  tools can generate token from claimset.
    /// </summary>
    [TestMethod]
    public void JwtToolsCanGenerateTokenFromClaimset()
    {
        byte[] securityKey = Convert.FromBase64String("OmuURxFCm3Vu4zXx8IqtHbhY8fsz9YtF++NDaD4rwj+nfqBU/ehaXbXMIjqAf51w3dnAFDmjWblW0EP6EuyE/w==");
        string result = CreateToken(securityKey);
        Assert.IsNotNull(result);

        Console.WriteLine(result);

        ClaimsPrincipal principal = JwtTools.GetPrincipalFromJwt(result, securityKey);
        Assert.IsNotNull(principal);
        Assert.IsTrue(principal.IsInRole("testuser"));
    }

    private static string CreateToken(byte[] securityKey)
    {
        Claim[] claims = new Claim[]
{
            new(ClaimType.PrimarySid.ClaimName(), Guid.NewGuid().ToString() ),
            new(ClaimType.Role.ClaimName(), "testuser"),
            new(ClaimType.UserName.ClaimName(), "TestUser"),
            new(ClaimType.Email.ClaimName(), "user@mycompany.local")
};
        return JwtTools.ConvertToJwt(claims, "http://localhost", "http://JwtToolsTests", securityKey, DateTime.UtcNow.AddMinutes(5));
    }
}