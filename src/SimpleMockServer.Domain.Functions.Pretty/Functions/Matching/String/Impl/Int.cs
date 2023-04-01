namespace SimpleMockServer.Domain.Functions.Pretty.Functions.Matching.String.Impl;

internal class Int : IStringMatchPrettyFunction
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
