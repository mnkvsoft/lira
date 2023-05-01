using SimpleMockServer.Domain.TextPart.Functions.Functions.Matching.String;

namespace SimpleMockServer.Domain.TextPart.Functions.Functions.Matching.String.Impl;

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
