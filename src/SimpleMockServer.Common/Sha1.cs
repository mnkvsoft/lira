using System.Security.Cryptography;
using System.Text;

namespace SimpleMockServer.Common;

public static class Sha1
{
    public static Hash Create(string text)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        return Create(bytes);
    }

    public static Hash Create(byte[] bytes)
    {
        using var sha1 = SHA1.Create();
        var hash = sha1.ComputeHash(bytes);
        return new Hash(hash);
    }

    public static Hash Create(Stream stream)
    {
        using var sha1 = SHA1.Create();
        var hash = sha1.ComputeHash(stream);
        return new Hash(hash);
    }
}
