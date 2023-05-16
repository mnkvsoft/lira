using SimpleMockServer.Domain.Matching.Request;

namespace SimpleMockServer.Domain.TextPart.Functions.Functions.Matching.String.Impl;

internal class Any : IStringMatchPrettyFunction
{
    public static string Name => "any";

    public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Any;

    public bool IsMatch(string? value)
    {
        return true;
    }

    public void SetArgument(string argument)
    {
        throw new NotImplementedException();
    }
}
