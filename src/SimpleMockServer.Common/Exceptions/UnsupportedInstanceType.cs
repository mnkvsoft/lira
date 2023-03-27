namespace SimpleMockServer.Common.Exceptions;

public class UnsupportedInstanceType : Exception
{
    public UnsupportedInstanceType(object instance) : base($"Unsupported instance type: " + instance.GetType().FullName)
    {

    }
}