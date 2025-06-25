namespace Lira.Domain.Configuration.Parsing;

static class ListTokenExtensions
{
    public static bool IsWhiteSpace(this IReadOnlyList<Token> tokens)
    {
        foreach (var token in tokens)
        {
            if(token is Token.StaticData staticData && staticData.Content.All(char.IsWhiteSpace))
                return true;
        }

        return false;
    }
}