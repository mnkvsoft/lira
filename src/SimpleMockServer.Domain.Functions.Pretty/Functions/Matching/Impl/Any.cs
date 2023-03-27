namespace SimpleMockServer.Domain.Functions.Pretty.Functions.Matching.Impl;

internal class Any : IMatchPrettyFunction
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
