using System.Text;

namespace SimpleMockServer.Common;

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
        return Enumerable.Range(0, hex.Length)
            .Where(x => x % 2 == 0)
            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
            .ToArray();
    }
}
