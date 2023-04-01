namespace SimpleMockServer.Domain.Functions.Pretty.Functions.Matching.String.Impl;

internal class Any : IStringMatchPrettyFunction
{
    public static string Name => "any";

    public bool IsMatch(string? value)
    {
        return true;
    }

    public void SetArgument(string argument)
    {
        throw new NotImplementedException();
    }
}
