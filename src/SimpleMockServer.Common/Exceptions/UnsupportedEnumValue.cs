namespace SimpleMockServer.Common.Exceptions;

public class UnsupportedEnumValue : Exception
{
    public UnsupportedEnumValue(Enum value) : base("Unsupported enum value: " + value)
    {

    }
}
