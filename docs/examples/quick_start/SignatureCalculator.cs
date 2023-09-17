using System.Security.Cryptography;
using System.Text;

namespace _;

public static class SignatureCalculator
{
    public static string Get(string text, string key)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(text + key);
        using var sha1 = SHA1.Create();
        byte[] hash = sha1.ComputeHash(bytes);

        var sb = new StringBuilder(bytes.Length * 2);

        foreach (var b in hash)
        {
            sb.Append(b.ToString("x2"));
        }

        return sb.ToString();
    }
}
