namespace SimpleMockServer.Common;

public static class GuidHelper
{
    public static Guid ToGuid(this long value)
    {
        byte[] guidData = new byte[16];
        Array.Copy(BitConverter.GetBytes(value), guidData, 8);
        return new Guid(guidData);
    }

    public static long ToLong(this Guid guid)
    {
        if (BitConverter.ToInt64(guid.ToByteArray(), 8) != 0)
            throw new OverflowException("Value was either too large or too small for an Int64.");
        return BitConverter.ToInt64(guid.ToByteArray(), 0);
    }
}