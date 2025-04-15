using System.IO.Abstractions;
using System.Security.Cryptography;

namespace N2.Core.Helpers;

public static class FileHelpers
{
    public static string ComputeSha384Hash(this IFileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);

        if (fileInfo.Exists)
        {
            return ComputeSha384Hash(fileInfo.FullName);
        }
        else
        {
            throw new NotSupportedException($"File not found: {fileInfo.FullName}");
        }
    }

    public static string ComputeSha384Hash(string filePath)
    {
        using (FileStream stream = File.OpenRead(filePath))
        {
            using (SHA384 sha384 = SHA384.Create())
            {
                byte[] hashBytes = sha384.ComputeHash(stream);
                return Convert.ToHexString(hashBytes)
                    .ToUpperInvariant();
            }
        }
    }
}