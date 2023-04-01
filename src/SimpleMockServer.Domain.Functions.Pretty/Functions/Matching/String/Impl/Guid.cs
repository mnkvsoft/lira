namespace SimpleMockServer.Domain.Functions.Pretty.Functions.Matching.String.Impl;

internal class Guid : IStringMatchPrettyFunction
{
    public static string Name => "guid";

    public bool IsMatch(string? value)
    {
        return System.Guid.TryParse(value, out _);
    }
}
