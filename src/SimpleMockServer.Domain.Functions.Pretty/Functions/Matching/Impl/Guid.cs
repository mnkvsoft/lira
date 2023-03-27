namespace SimpleMockServer.Domain.Functions.Pretty.Functions.Matching.Impl;

internal class Guid : IMatchPrettyFunction
{
    public static string Name => "guid";

    public bool IsMatch(string? value)
    {
        return System.Guid.TryParse(value, out _);
    }
}
