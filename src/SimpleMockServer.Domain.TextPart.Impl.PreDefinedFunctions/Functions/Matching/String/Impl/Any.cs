using SimpleMockServer.Domain.Matching.Request;

namespace SimpleMockServer.Domain.TextPart.PreDefinedFunctions.Functions.Matching.String.Impl;

internal class Any : IStringMatchPrettyFunction
{
    public static string Name => "any";

    public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Any;

    public bool IsMatch(string? value)
    {
        return true;
    }
}
