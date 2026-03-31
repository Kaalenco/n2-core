using System.Security.Cryptography;

namespace N2.Core.Extensions;

public static class RandomStringGenerator
{

    /// <summary>
    /// Generate a random string without characters that could be confused
    /// </summary>
    /// <remarks>
    /// Do not use the string for security purposes.
    /// </remarks>
    public static string Generate(int length)
    {
        // The characters that are allowed in the random string Characters
        // that are easily confused are not part of the set.
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
#if NETSTANDARD2_1_OR_GREATER
        return new string([..
            Enumerable.Repeat(chars, length)
            .Select(s => s[RandomNumberGenerator.GetInt32(s.Length)])]);
#else
        return new string([..
            Enumerable.Repeat(chars, length)
            .Select(s => s[GetInt32(s.Length)])]);
#endif
    }

#if NETSTANDARD2_1_OR_GREATER
    // This method is not needed in .NET Standard 2.1 or later because RandomNumberGenerator.GetInt32 is available.
#else
    internal static Int32 GetInt32(int maxValue)
    {
        if (maxValue <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxValue), "maxValue must be greater than 0.");
        }
        using var r = RandomNumberGenerator.Create();
        byte[] bytes = new byte[4];
        r.GetBytes(bytes);
        int value = BitConverter.ToInt32(bytes, 0) & int.MaxValue; // Ensure non-negative
        return value % maxValue;
    }
#endif
}