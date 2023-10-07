using System.Globalization;
using System.Text;

namespace Lira.Common;

public static class HexConverter
{
    public static string ToHexString(byte[] bytes)
    {
        var sb = new StringBuilder(bytes.Length * 2);

        foreach (var b in bytes)
        {
            sb.Append(b.ToString("x2"));
        }

        return sb.ToString();
    }

    public static byte[] ToBytes(string hex)
    {
        if (!IsValidHexString(hex))
            throw new ArgumentException($"Invalid hex string '{hex}'");

        return Enumerable.Range(0, hex.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                     .ToArray();
    }

    public static bool IsValidHexString(string hex)
    {
        if (hex.Length % 2 != 0)
            return false;

        for (int i = 0; i < hex.Length; i++)
        {
            if (i % 2 != 0)
                continue;

            if (!byte.TryParse(hex.AsSpan(i, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte b))
                return false;
        }

        return true;
    }
}
