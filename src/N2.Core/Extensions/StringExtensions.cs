using System.Globalization;

namespace N2.Core.Extensions;

public static class StringExtensions
{
    public static int CheckBalance(this string input, char start, char end)
    {
        if (string.IsNullOrEmpty(input))
        {
            return 0;
        }
        int balance = 0;
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            if (c == start)
            {
                balance++;
            }
            else if (c == end)
            {
                balance--;
            }
        }
        return balance;
    }

    /// <summary>
    /// Find a part of an HTML document using the tag name.
    /// </summary>
    public static string FindHtmlPart(string tagName, string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return string.Empty;
        }
        const StringComparison compare = StringComparison.InvariantCultureIgnoreCase;
        const string endStartTag = ">";
        string startTag = $"<{tagName}";
        string endTag = $"</{tagName}>";
        int start = content.IndexOf(startTag, compare);
        if (start == -1)
        {
            return string.Empty;
        }

        start = content.IndexOf(endStartTag, start, compare) + 1;
        int end = content.IndexOf(endTag, start, compare);
        if (end == -1)
        {
            return string.Empty;
        }

#if NETSTANDARD2_0
        return content.Substring(start, end - start);
#else
        return content[start..end];
#endif
    }

    /// <summary>
    /// Parse a sttring to a DateTimeOffset and convert it to local time.
    /// </summary>
    /// <param name="input">The string containing a date and time</param>
    /// <param name="format">The formatter to compensate for the expected string value</param>
    /// <returns>A date time, relative to UTC</returns>
    public static DateTimeOffset ParseStringToLocalTime(this string input, IFormatProvider format)
    {
        DateTimeOffset dateTimeOffset = DateTimeOffset.Parse(input, format);
        return dateTimeOffset.AddHours(dateTimeOffset.Offset.Hours);
    }

    private static readonly char[] sanitizeCharacters = new[] { ' ', '.', '<', '>', ':', '"', '/', '\\', '|', '?', '*' };

    /// <summary>
    /// Remove illegal characters from a file name.
    /// The filename should not contain path information.
    /// The extension on the filename is allowed.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static string SanitizeFileName(this string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return string.Empty;
        }
#pragma warning disable CA1308 // Normalize strings to uppercase
        return fileName
            .ReplaceChars(sanitizeCharacters, '-')
            .ToLower(CultureInfo.InvariantCulture);
    }

    public static string ReplaceChars(this string value, char[] oldChars, char newChar)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }
        var result = new char[value.Length];
        for(int i = 0; i < result.Length; i++)
        {
            var c = value[i];
            if(oldChars.Contains(c))
            {
                result[i] = newChar;
            }
            else
            {
                result[i] = c;
            }
        }
        return new string(result);
    }

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
                {
                    break;
                }

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

#if NETSTANDARD2_0
        var rightText = value.Substring(1);
#else
        string rightText = value[1..];
#endif

#pragma warning disable CA1308 // Normalize strings to uppercase
        return char.ToUpper(value[0], CultureInfo.InvariantCulture) + rightText.ToLowerInvariant();
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

#if NETSTANDARD2_0
        return value.Substring(0, length);
#else
        return value[..length];
#endif
    }

    public static Guid ConvertToGuid(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return Guid.Empty;
        }
        if (Guid.TryParse(value, out Guid result))
        {
            return result;
        }

        // Convert the string to a guid, if it fits
        byte[] byteData = value.ToUpperInvariant().Select(c => (byte)c).ToArray();
        if (byteData.Length > 11)
        {
            byteData = byteData.Take(11).ToArray();
        }
        byte[] guidData = new byte[16];
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
        byte[] bytes = value.ToByteArray();
        byte length = bytes[11];
        if (bytes[12] == 0xda && bytes[13] == 0xda && bytes[14] == 0x00 && bytes[15] == 0xff)
        {
            return new string(bytes.Take(length).Select(b => (char)b).ToArray());
        }
        return value.ToString();
    }

    public static bool IsEnum<T>(this string value, T enumValue) where T : struct
    {
        if (Enum.TryParse<T>(value, true, out T tValue))
        {
            return tValue.Equals(enumValue);
        }
        return false;
    }

    /// <summary>
    /// A Soundex code is a hashing mechanism that defines a code for the way a word
    /// is pronounced. It provides the possibility to compare two word equality without
    /// the need to be totally equal.
    /// </summary>
    /// <param name="data">The oroiginal word</param>
    /// <returns>A soundex string</returns>
    /// <remarks> From : https://stackoverflow.com/questions/11121936/dotnet-soundex-function</remarks>
    public static string Soundex(this string data)
    {
        char[] result = new char[] { '0', '0', '0', '0', };

        if (data != null && data.Length > 0)
        {
            char previousCode = '\0', currentCode = '\0', currentLetter = '\0';
            int n = 0;
            result[n++] = char.ToUpperInvariant(data[0]);

            for (int i = 0; i < data.Length; i++)
            {
                currentLetter = char.ToUpperInvariant(data[i]);
                currentCode = '\0';

                if (ContainsCharacter("BFPV", currentLetter))
                {
                    currentCode = '1';
                }
                else if (ContainsCharacter("CGJKQSXZ", currentLetter))
                {
                    currentCode = '2';
                }
                else if (ContainsCharacter("DT", currentLetter))
                {
                    currentCode = '3';
                }
                else if (currentLetter == 'L')
                {
                    currentCode = '4';
                }
                else if (ContainsCharacter("MN", currentLetter))
                {
                    currentCode = '5';
                }
                else if (currentLetter == 'R')
                {
                    currentCode = '6';
                }

                if (currentCode != previousCode && i > 0 && currentCode != '\0')
                {
                    result[n++] = currentCode;
                }

                if (n == 4)
                {
                    break;
                }

                previousCode = currentCode;
            }
        }

        return new string(result);
    }

    private static bool ContainsCharacter(string data, char c)
    {
        if (data == null || data.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < data.Length; i++)
        {
            if (char.Equals(data[i], c))
            {
                return true;
            }
        }
        return false;
    }
}
