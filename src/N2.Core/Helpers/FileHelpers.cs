using System.Globalization;
using System.IO.Abstractions;
using System.Security.Cryptography;
using System.Text;

namespace N2.Core.Helpers;

public static class FileHelpers
{
    public static string ComputeSha384Hash(this IFileInfo fileInfo)
    {
        Contract.NotNull(fileInfo, nameof(fileInfo));

        if (fileInfo.Exists)
        {
            using Stream stream = fileInfo.OpenRead();
            return ComputeSha384Hash(stream);
        }
        else
        {
            throw new NotSupportedException($"File not found: {fileInfo.FullName}");
        }
    }

    public static string ComputeSha384Hash(IFileInfoFactory fileInfoFactory, string filePath)
    {
        Contract.NotNull(fileInfoFactory, nameof(fileInfoFactory));
        IFileInfo fileInfo = fileInfoFactory.New(filePath);
        return ComputeSha384Hash(fileInfo);
    }

    public static string ComputeSha384Hash(Stream stream)
    {
        using (SHA384 sha384 = SHA384.Create())
        {
            byte[] hashBytes = sha384.ComputeHash(stream);
            return ToHexString(hashBytes)
                .ToUpperInvariant();
        }
    }

    public static string ToHexString(this byte[] bytes)
    {
        Contract.NotNull(bytes, nameof(bytes));
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (byte b in bytes)
        {
            sb.Append(b.ToString("X2", CultureInfo.InvariantCulture));
        }
        return sb.ToString();
    }
}