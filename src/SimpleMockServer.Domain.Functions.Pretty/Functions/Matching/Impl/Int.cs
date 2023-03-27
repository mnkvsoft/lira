using SimpleMockServer.Domain.Functions.Pretty.Functions.Matching;

namespace SimpleMockServer.Domain.Functions.Pretty.Functions.Matching.Impl;

internal class Int : IMatchPrettyFunction
{
    public static string Name => "int";

    public bool IsMatch(string? value)
    {
        return long.TryParse(value, out _);
    }

    public void SetArgument(string argument)
    {
        throw new NotImplementedException();
    }
}
