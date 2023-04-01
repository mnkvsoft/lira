namespace SimpleMockServer.Domain.Functions.Pretty.Functions.Matching.Int.Impl;

internal class Equals : IIntMatchPrettyFunction, IWithIntArgumenFunction
{
    public static string Name => "equals";
    int _argument;

    public bool IsMatch(int value)
    {
        return _argument == value;
    }

    public void SetArgument(int argument)
    {
        _argument = argument;
    }
}
