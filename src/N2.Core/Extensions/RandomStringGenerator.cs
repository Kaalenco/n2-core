namespace N2.Core.Extensions;

#pragma warning disable CA5394 // Mark members as static
public static class RandomStringGenerator
{
    private static readonly Random random = new();

    /// <summary>
    /// Generate a random string without characters that could be confused
    /// </summary>
    /// <remarks>
    /// Do not use the string for security purposes.
    /// </remarks>
    public static string Generate(int length)
    {
        // The characters that are allowed in the random string
        // Characters that are easily confused are removed
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
        return new string(Enumerable.Repeat(chars, length)
                     .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
