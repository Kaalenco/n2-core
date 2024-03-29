﻿using System.Globalization;

namespace N2.Core.Extensions;
public static class StringExtensions
{
    public static int GetStableHashCode(this string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return 0;
        }
        unchecked
        {
            int hash1 = 5381;
            int hash2 = hash1;

            for (int i = 0; i < str.Length && str[i] != '\0'; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if (i == str.Length - 1 || str[i + 1] == '\0')
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
            }

            return hash1 + (hash2 * 1566083941);
        }
    }

    public static string UppercaseFirst(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }
#pragma warning disable CA1308 // Normalize strings to uppercase
        return char.ToUpper(value[0], CultureInfo.InvariantCulture) + value[1..].ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase
    }

    public static string Truncate(this string value, int length)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }
        if (value.Length <= length)
        {
            return value;
        }
        return value[..length];
    }

    public static Guid ConvertToGuid(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return Guid.Empty;
        }
        if (Guid.TryParse(value, out var result))
        {
            return result;
        }

        // Convert the string to a guid, if it fits
        var byteData = value.ToUpperInvariant().Select(c => (byte)c).ToArray();
        if (byteData.Length > 11)
        {
            byteData = byteData.Take(11).ToArray();
        }
        var guidData = new byte[16];
        byteData.CopyTo(guidData, 0);
        guidData[15] = 0xff;
        guidData[14] = 0x00;
        guidData[13] = 0xda;
        guidData[12] = 0xda;
        guidData[11] = (byte)byteData.Length;
        return new Guid(guidData);
    }

    public static string ConvertToString(this Guid value)
    {
        if (value == Guid.Empty)
        {
            return string.Empty;
        }
        var bytes = value.ToByteArray();
        var length = bytes[11];
        if (bytes[12] == 0xda && bytes[13] == 0xda && bytes[14] == 0x00 && bytes[15] == 0xff)
        {
            return new string(bytes.Take(length).Select(b => (char)b).ToArray());
        }
        return value.ToString();
    }
}