namespace SimpleMockServer.Common;

public static class GuidHelper
{
    public static Guid ToRandomGuid(this long value)
    {
        byte[] result = new byte[16];
        var randomPart = result.AsSpan(0, 8);
        Random.Shared.NextBytes(randomPart);
        
        Array.Copy(BitConverter.GetBytes(value), 0, result, 8, 8);
        
        return new Guid(result);
    }

    public static long ToInt64(this Guid randomGuid) => BitConverter.ToInt64(randomGuid.ToByteArray().AsSpan(8, 8));
}