namespace SimpleMockServer.Common;

public static class GuidHelper
{
    public static Guid ToGuid(this long value)
    {
        byte[] valueBytes = BitConverter.GetBytes(value);
        byte[] randomBytes = new byte[8];
        
        Random.Shared.NextBytes(randomBytes);

        byte[] result = new byte[16];
        for (int i = 0; i < randomBytes.Length; i++)
        {
            result[i * 2] = valueBytes[i];
            result[i * 2 + 1] = randomBytes[i];
        }
        
        return new Guid(result);
    }

    public static long ToLong(this Guid guid)
    {
        var bytes = guid.ToByteArray();
        byte[] result = new byte[8];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = bytes[i * 2];
        }
        
        return BitConverter.ToInt64(result);
    }
}