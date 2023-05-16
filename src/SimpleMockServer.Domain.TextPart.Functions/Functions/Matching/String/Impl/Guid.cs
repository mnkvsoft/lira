using SimpleMockServer.Domain.Matching.Request;

namespace SimpleMockServer.Domain.TextPart.Functions.Functions.Matching.String.Impl;

internal class Guid : IStringMatchPrettyFunction
{
    
    public static string Name => "guid";

    public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Type;

    public bool IsMatch(string? value)
    {
        return System.Guid.TryParse(value, out _);
    }
}
