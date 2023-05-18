using System.Security.Cryptography;
using System.Text;

namespace SimpleMockServer.Common;

public static class HashUtils
{
    public static string GetSha1(string text)
    {
        using var sha1 = SHA1.Create();

        byte[] bytes = Encoding.UTF8.GetBytes(text);
        var hash = sha1.ComputeHash(bytes);
        var sb = new StringBuilder(hash.Length * 2);

        foreach (var b in hash)
        {
            sb.Append(b.ToString("x2"));
        }

        return sb.ToString();
    }
}
